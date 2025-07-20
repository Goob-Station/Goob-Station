// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aineias1 <dmitri.s.kiselev@gmail.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 McBosserson <148172569+McBosserson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <plmilonpl@gmail.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Unlumination <144041835+Unlumy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Lavaland.Hierophant.Components;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Timing;
using Content.Shared._Lavaland.Hierophant.Systems;
using Content.Shared._Lavaland.Movement;
using Content.Shared._Lavaland.Tile;
using Content.Shared.Chat;
using Content.Shared.Hands.EntitySystems;
using Robust.Shared.Network;

namespace Content.Shared._Lavaland.HierophantClub;

public sealed class HierophandClubItemSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedChatSystem _chat = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly HierophantSystem _hierophant = default!;
    [Dependency] private readonly TileShapeSystem _tile = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HierophantClubItemComponent, HierophantClubActivateCrossEvent>(OnCreateCross);
        SubscribeLocalEvent<HierophantClubItemComponent, HierophantClubPlaceMarkerEvent>(OnPlaceMarker);
        SubscribeLocalEvent<HierophantClubItemComponent, HierophantClubTeleportToMarkerEvent>(OnTeleport);
        SubscribeLocalEvent<HierophantClubItemComponent, HierophantClubToggleTileMovementEvent>(OnToggleTileMovement);
    }

    private void OnCreateCross(Entity<HierophantClubItemComponent> ent, ref HierophantClubActivateCrossEvent args)
    {
        if (args.Handled || !args.Target.IsValid(EntityManager))
            return;

        var uid = ent.Owner;
        var user = args.Performer;

        if (!_hands.IsHolding(user, uid, out _))
        {
            _popup.PopupEntity(Loc.GetString("dash-ability-not-held", ("item", uid)), user, user);
            return;
        }

        var targetCoords = args.Target;
        SpawnHierophantCross(user, targetCoords, ent.Comp);

        args.Handled = true;
    }

    private void OnPlaceMarker(Entity<HierophantClubItemComponent> ent, ref HierophantClubPlaceMarkerEvent args)
    {
        if (args.Handled)
            return;

        var user = args.Performer;

        if (_net.IsServer)
            QueueDel(ent.Comp.TeleportMarker);

        var position = Transform(args.Performer)
            .Coordinates
            .SnapToGrid(EntityManager, _mapMan);

        var marker = PredictedSpawnAtPosition(ent.Comp.TeleportMarkerPrototype, position);
        ent.Comp.TeleportMarker = marker;
        ent.Comp.TeleportCoordinates = position;

        _popup.PopupPredicted("Teleportation point set.", user, user);

        AddImmunity(user);
        _tile.SpawnTileShape(ent.Comp.BlinkShape, position, ent.Comp.HierophantDamageTileId, out _);

        args.Handled = true;
    }

    private void OnTeleport(Entity<HierophantClubItemComponent> ent, ref HierophantClubTeleportToMarkerEvent args)
    {
        if (args.Handled)
            return;

        var comp = ent.Comp;
        if (comp.TeleportMarker == null
            || comp.TeleportCoordinates == null)
        {
            _popup.PopupClient("Marker is not placed!", args.Performer, PopupType.MediumCaution);
            return;
        }

        var user = args.Performer;

        AddImmunity(user);
        _hierophant.Blink(user, comp.HierophantDamageTileId, comp.BlinkShape, comp.TeleportCoordinates.Value);

        if (_net.IsServer)
            QueueDel(comp.TeleportMarker);

        comp.TeleportMarker = null;
        comp.TeleportCoordinates = null;

        args.Handled = true;
    }

    private void OnToggleTileMovement(Entity<HierophantClubItemComponent> ent, ref HierophantClubToggleTileMovementEvent args)
    {
        if (args.Handled)
            return;

        if (HasComp<HierophantBeatComponent>(args.Target))
            RemComp<HierophantBeatComponent>(args.Target);
        else
            EnsureComp<HierophantBeatComponent>(args.Target);

        _chat.TrySendInGameICMessage(args.Performer, Loc.GetString("action-hierophant-tile-movement-cast"), InGameICChatType.Speak, false);
        args.Handled = true;
    }

    private void SpawnHierophantCross(EntityUid owner, EntityCoordinates coords, HierophantClubItemComponent club)
    {
        AddImmunity(owner);
        _tile.SpawnTileShape(club.CrossAttackShape, coords, club.HierophantDamageTileId, out _);
        _audio.PlayPredicted(club.DamageSound, coords, owner);
    }

    private void AddImmunity(EntityUid uid, float time = 3f)
        => EnsureComp<HierophantImmuneComponent>(uid).EndTime = _timing.CurTime + TimeSpan.FromSeconds(time);
}
