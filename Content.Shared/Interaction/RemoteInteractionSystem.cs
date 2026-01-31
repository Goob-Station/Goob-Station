// SPDX-FileCopyrightText: 2025 GoobStation
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Interaction.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Content.Shared.Popups;
using Content.Shared.Coordinates;

namespace Content.Shared.Interaction;

public sealed class RemoteInteractionSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _powerReceiver = default!;
    [Dependency] private readonly StationAiVisionSystem _vision = default!;
    [Dependency] private readonly SharedMapSystem _maps = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;

    private EntityQuery<BroadphaseComponent> _broadphaseQuery;
    private EntityQuery<MapGridComponent> _gridQuery;

    public override void Initialize()
    {
        base.Initialize();

        _broadphaseQuery = GetEntityQuery<BroadphaseComponent>();
        _gridQuery = GetEntityQuery<MapGridComponent>();

        SubscribeLocalEvent<RemoteInteractionComponent, InRangeOverrideEvent>(OnRemoteInRange);
        SubscribeLocalEvent<RemoteInteractionComponent, InteractionUsingAttemptEvent>(OnRemoteInteractionUsingAttempt);
        SubscribeLocalEvent<RemoteInteractionComponent, InteractionAttemptEvent>(OnRemoteInteractionAttempt);
    }

    private void OnRemoteInteractionAttempt(Entity<RemoteInteractionComponent> ent, ref InteractionAttemptEvent args)
    {
        if (args.Target == null || args.Target == ent.Owner)
            return;

        if (!TryComp(args.Target, out StationAiWhitelistComponent? whitelistComponent))
        {
            return;
        }

        if (IsInNormalRange(args.Uid, args.Target.Value))
        {
            return;
        }

        if (whitelistComponent is { Enabled: false } || !_powerReceiver.IsPowered(args.Target.Value))
        {
            ShowDeviceNotRespondingPopup(args.Uid);
            args.Cancelled = true;
            return;
        }
    }

    private void OnRemoteInteractionUsingAttempt(Entity<RemoteInteractionComponent> ent, ref InteractionUsingAttemptEvent args)
    {
        if (args.Target == null || args.Target == ent.Owner)
            return;

        if (!IsInNormalRange(args.Uid, args.Target.Value) && args.Used != null)
        {
            var test = IsInNormalRange(args.Uid, args.Target.Value);
            ShowDeviceTooFarPopup(args.Uid);
            args.Cancelled = true;
            return;
        }
    }

    private bool IsInNormalRange(EntityUid user, EntityUid target)
    {
        if (!TryComp(user, out TransformComponent? userXForm) || !TryComp(target, out TransformComponent? targetXForm))
            return false;

        return _interaction.InRangeUnobstructed(
            (user, userXForm),
            (target, targetXForm),
            targetXForm.Coordinates,
            targetXForm.LocalRotation);
    }

    private void OnRemoteInRange(Entity<RemoteInteractionComponent> ent, ref InRangeOverrideEvent args)
    {
        var targetXForm = Transform(args.Target);

        if (targetXForm.GridUid != Transform(args.User).GridUid)
        {
            return;
        }

        if (!TryComp(args.Target, out StationAiWhitelistComponent? whitelistComponent) ||
            whitelistComponent is { Enabled: false } ||
            !_powerReceiver.IsPowered(args.Target))
        {
            return;
        }

        if (IsInNormalRange(args.User, args.Target))
        {
            return;
        }

        if (!_broadphaseQuery.TryComp(targetXForm.GridUid, out var broadphase) ||
            !_gridQuery.TryComp(targetXForm.GridUid, out var grid))
        {
            args.InRange = false;
            args.Handled = true;
            return;
        }

        var targetTile = _maps.LocalToTile(targetXForm.GridUid.Value, grid, targetXForm.Coordinates);

        lock (_vision)
        {
            args.InRange = _vision.IsAccessible((targetXForm.GridUid.Value, broadphase, grid), targetTile);
        }

        args.Handled = true;
    }

    private void ShowDeviceNotRespondingPopup(EntityUid toEntity)
    {
        _popup.PopupClient(Loc.GetString("ai-device-not-responding"), toEntity, PopupType.MediumCaution);
    }

    private void ShowDeviceTooFarPopup(EntityUid toEntity)
    {
        _popup.PopupClient(Loc.GetString("ai-device-too-far"), toEntity, PopupType.MediumCaution);
    }
}
