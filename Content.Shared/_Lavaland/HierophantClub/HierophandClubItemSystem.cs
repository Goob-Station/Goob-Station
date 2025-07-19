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

using Content.Shared._Lavaland.Damage;
using Content.Shared.Actions;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Timing;
using Content.Shared._Lavaland.Hierophant.Systems;
using Content.Shared._Lavaland.Movement;
using Content.Shared._Lavaland.Tile;
using Content.Shared.Chat;
using Content.Shared.Hands.EntitySystems;

namespace Content.Shared._Lavaland.HierophantClub;

public sealed class HierophandClubItemSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedChatSystem _chat = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly HierophantSystem _hierophant = default!;
    [Dependency] private readonly TileShapeSystem _tile = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HierophantClubItemComponent, MapInitEvent>(OnClubInit);
        SubscribeLocalEvent<HierophantClubItemComponent, GetItemActionsEvent>(OnGetActions);

        SubscribeLocalEvent<HierophantClubItemComponent, HierophantClubActivateCrossEvent>(OnCreateCross);
        SubscribeLocalEvent<HierophantClubItemComponent, HierophantClubPlaceMarkerEvent>(OnPlaceMarker);
        SubscribeLocalEvent<HierophantClubItemComponent, HierophantClubTeleportToMarkerEvent>(OnTeleport);
        SubscribeLocalEvent<HierophantClubItemComponent, HierophantClubToggleTileMovementEvent>(OnToggleTileMovement);
    }

    private void OnClubInit(Entity<HierophantClubItemComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent, ref ent.Comp.CreateCrossActionEntity, ent.Comp.CreateCrossActionId);
        _actions.AddAction(ent, ref ent.Comp.PlaceMarkerActionEntity, ent.Comp.PlaceMarkerActionId);
        _actions.AddAction(ent, ref ent.Comp.TeleportToMarkerActionEntity, ent.Comp.TeleportToMarkerActionId);
        _actions.AddAction(ent, ref ent.Comp.ToggleTileMovementActionEntity, ent.Comp.ToggleTileMovementActionId);
    }

    private void OnGetActions(Entity<HierophantClubItemComponent> ent, ref GetItemActionsEvent args)
    {
        args.AddAction(ref ent.Comp.CreateCrossActionEntity, ent.Comp.CreateCrossActionId);
        args.AddAction(ref ent.Comp.PlaceMarkerActionEntity, ent.Comp.PlaceMarkerActionId);
        args.AddAction(ref ent.Comp.TeleportToMarkerActionEntity, ent.Comp.TeleportToMarkerActionId);
        args.AddAction(ref ent.Comp.ToggleTileMovementActionEntity, ent.Comp.ToggleTileMovementActionId);
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

        var targetCoords = args.Target.SnapToGrid(EntityManager, _mapMan);

        SpawnHierophantCross(user, targetCoords, ent.Comp);

        args.Handled = true;
    }

    private void OnPlaceMarker(Entity<HierophantClubItemComponent> ent, ref HierophantClubPlaceMarkerEvent args)
    {
        if (args.Handled)
            return;

        var user = args.Performer;

        PredictedQueueDel(ent.Comp.TeleportMarker);

        var position = Transform(args.Performer)
            .Coordinates
            .SnapToGrid(EntityManager, _mapMan);

        var marker = PredictedSpawnAtPosition(ent.Comp.TeleportMarkerPrototype, position);
        ent.Comp.TeleportMarker = marker;

        _popup.PopupEntity("Teleportation point set.", user);

        AddImmunity(user);
        _tile.SpawnTileShape(ent.Comp.BlinkShape, position, ent.Comp.HierophantDamageTileId, out _);

        args.Handled = true;
    }

    private void OnTeleport(Entity<HierophantClubItemComponent> ent, ref HierophantClubTeleportToMarkerEvent args)
    {
        if (args.Handled)
            return;

        if (ent.Comp.TeleportMarker == null)
        {
            _popup.PopupClient("Marker is not placed!", args.Performer, PopupType.MediumCaution);
            return;
        }

        var user = args.Performer;
        AddImmunity(user);

        var position = Transform(ent.Comp.TeleportMarker.Value).Coordinates;
        _hierophant.Blink(user, ent.Comp.HierophantDamageTileId, ent.Comp.BlinkShape, position);

        args.Handled = true;
    }

    private void OnToggleTileMovement(Entity<HierophantClubItemComponent> ent, ref HierophantClubToggleTileMovementEvent args)
    {
        if (args.Handled || TerminatingOrDeleted(args.Target))
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
        _audio.PlayPredicted(club.DamageSound, coords, owner, AudioParams.Default.WithMaxDistance(10f));
    }

    private void AddImmunity(EntityUid uid, float time = 3f)
        => EnsureComp<DamageSquareImmunityComponent>(uid).HasImmunityUntil = _timing.CurTime + TimeSpan.FromSeconds(time);
}
