using Content.Server.Popups;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Examine;
using Content.Shared.Interaction.Events;
using Content.Shared.Maps;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Physics;
using Content.Shared.WhiteDream.BloodCult;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Server.WhiteDream.BloodCult.Items.VeilShifter;

public sealed class VeilShifterSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VeilShifterComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<VeilShifterComponent, UseInHandEvent>(OnUseInHand);
    }

    private void OnExamined(Entity<VeilShifterComponent> veil, ref ExaminedEvent args) =>
        args.PushMarkup(Loc.GetString("veil-shifter-description", ("charges", veil.Comp.Charges)));

    private void OnUseInHand(Entity<VeilShifterComponent> veil, ref UseInHandEvent args)
    {
        if (args.Handled || veil.Comp.Charges == 0 || !Teleport(veil, args.User))
            return;

        veil.Comp.Charges--;
        if (veil.Comp.Charges == 0)
            _appearance.SetData(veil, GenericCultVisuals.State, false);

        args.Handled = true;
    }

    private bool Teleport(Entity<VeilShifterComponent> veil, EntityUid user)
    {
        var userTransform = Transform(user);

        EntityCoordinates coords = default;
        var oldCoords = userTransform.Coordinates;
        var direction = userTransform.LocalRotation.GetDir().ToVec();
        var offset = userTransform.LocalRotation.ToWorldVec().Normalized();

        var foundPos = false;

        for (var i = 0; i < veil.Comp.Attempts; i++)
        {
            var distance = _random.Next(veil.Comp.TeleportDistanceMin, veil.Comp.TeleportDistanceMax);
            coords = userTransform.Coordinates.Offset(offset + direction * distance).SnapToGrid();

            if (!coords.TryGetTileRef(out var tileRef) || _turf.IsTileBlocked(tileRef.Value, CollisionGroup.MobMask))
                continue;
            foundPos = true;
            break;
        }

        if (!foundPos)
        {
            _popup.PopupClient(Loc.GetString("veil-shifter-cant-teleport"), veil, user);
            return false;
        }

        if (_pulling.TryGetPulledEntity(user, out var pulledEntity))
            _pulling.TryStopPull(pulledEntity.Value);

        _transform.SetCoordinates(user, coords);
        if (pulledEntity.HasValue)
        {
            _transform.SetCoordinates(pulledEntity.Value, coords);
            _pulling.TryStartPull(user, pulledEntity.Value);
        }

        _audio.PlayPvs(veil.Comp.TeleportInSound, coords);
        _audio.PlayPvs(veil.Comp.TeleportOutSound, oldCoords);

        Spawn(veil.Comp.TeleportInEffect, coords);
        Spawn(veil.Comp.TeleportOutEffect, oldCoords);
        return true;
    }
}
