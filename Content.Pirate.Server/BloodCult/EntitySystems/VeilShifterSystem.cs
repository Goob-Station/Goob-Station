// SPDX-FileCopyrightText: 2024 Remuchi <72476615+Remuchi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.BloodCult.Components;
using Content.Server.Popups;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Examine;
using Content.Shared.Interaction.Events;
using Content.Shared.Maps;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Physics;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Server.BloodCult.EntitySystems;

public sealed class VeilShifterSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VeilShifterComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<VeilShifterComponent, UseInHandEvent>(OnUseInHand);
    }

    private void OnExamined(Entity<VeilShifterComponent> veil, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("veil-shifter-description", ("charges", veil.Comp.Charges)));
    }

    private void OnUseInHand(Entity<VeilShifterComponent> veil, ref UseInHandEvent args)
    {
        if (args.Handled || veil.Comp.Charges <= 0 ||
            (!HasComp<BloodCultistComponent>(args.User) && !HasComp<BloodCultConstructComponent>(args.User)) ||
            !TryTeleport(veil, args.User))
            return;

        veil.Comp.Charges--;
        if (veil.Comp.Charges == 0)
            _appearance.SetData(veil, BloodCultVisuals.Active, false);

        args.Handled = true;
    }

    private bool TryTeleport(Entity<VeilShifterComponent> veil, EntityUid user)
    {
        var userTransform = Transform(user);
        var oldCoordinates = userTransform.Coordinates;
        var direction = userTransform.LocalRotation.ToWorldVec().Normalized();
        EntityCoordinates destination = default;
        var foundDestination = false;

        for (var i = 0; i < veil.Comp.Attempts; i++)
        {
            var distance = _random.Next(veil.Comp.TeleportDistanceMin, veil.Comp.TeleportDistanceMax + 1);
            destination = oldCoordinates.Offset(direction * distance).SnapToGrid();

            if (!_turf.TryGetTileRef(destination, out var tile) || _turf.IsTileBlocked(tile.Value, CollisionGroup.MobMask))
                continue;

            foundDestination = true;
            break;
        }

        if (!foundDestination)
        {
            _popup.PopupEntity(Loc.GetString("veil-shifter-cant-teleport"), veil, user);
            return false;
        }

        var pulled = _pulling.GetPulling(user);
        _pulling.StopAllPulls(user);

        _transform.SetCoordinates(user, destination);
        if (pulled is { } pulledEntity)
        {
            _transform.SetCoordinates(pulledEntity, destination);
            _pulling.TryStartPull(user, pulledEntity);
        }

        _audio.PlayPvs(veil.Comp.TeleportInSound, destination);
        _audio.PlayPvs(veil.Comp.TeleportOutSound, oldCoordinates);

        if (veil.Comp.TeleportInEffect is { } teleportIn)
            Spawn(teleportIn, destination);

        if (veil.Comp.TeleportOutEffect is { } teleportOut)
            Spawn(teleportOut, oldCoordinates);

        return true;
    }
}
