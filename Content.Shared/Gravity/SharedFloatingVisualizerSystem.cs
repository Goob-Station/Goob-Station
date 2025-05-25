// SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Adeinitas <147965189+adeinitas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Danger Revolution! <142105406+DangerRevolution@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 Timemaster99 <57200767+Timemaster99@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Robust.Shared.Map;
using Content.Shared._EinsteinEngines.Flight.Events; // Goobstation

namespace Content.Shared.Gravity;

/// <summary>
/// Handles offsetting a sprite when there is no gravity
/// </summary>
public abstract class SharedFloatingVisualizerSystem : EntitySystem
{
    [Dependency] private readonly SharedGravitySystem GravitySystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FloatingVisualsComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<GravityChangedEvent>(OnGravityChanged);
        SubscribeLocalEvent<FloatingVisualsComponent, EntParentChangedMessage>(OnEntParentChanged);
        SubscribeNetworkEvent<FlightEvent>(OnFlight);
    }

    /// <summary>
    /// Offsets a sprite with a linear interpolation animation
    /// </summary>
    public virtual void FloatAnimation(EntityUid uid, Vector2 offset, string animationKey, float animationTime, bool stop = false) { }

    protected bool CanFloat(EntityUid uid, FloatingVisualsComponent component, TransformComponent? transform = null)
    {
        if (!Resolve(uid, ref transform))
            return false;

        if (transform.MapID == MapId.Nullspace)
            return false;

        component.CanFloat = GravitySystem.IsWeightless(uid, xform: transform);
        Dirty(uid, component);
        return component.CanFloat;
    }

    private void OnComponentStartup(EntityUid uid, FloatingVisualsComponent component, ComponentStartup args)
    {
        if (CanFloat(uid, component))
            FloatAnimation(uid, component.Offset, component.AnimationKey, component.AnimationTime);
    }

    private void OnGravityChanged(ref GravityChangedEvent args)
    {
        var query = EntityQueryEnumerator<FloatingVisualsComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var floating, out var transform))
        {
            if (transform.MapID == MapId.Nullspace)
                continue;

            if (transform.GridUid != args.ChangedGridIndex)
                continue;

            floating.CanFloat = !args.HasGravity;
            Dirty(uid, floating);

            if (!args.HasGravity)
                FloatAnimation(uid, floating.Offset, floating.AnimationKey, floating.AnimationTime);
        }
    }

    private void OnFlight(FlightEvent args)
    {
        var uid = GetEntity(args.Uid);
        if (!TryComp<FloatingVisualsComponent>(uid, out var floating))
            return;
        floating.CanFloat = args.IsFlying;

        if (!args.IsFlying
            || !args.IsAnimated)
            return;

        FloatAnimation(uid, floating.Offset, floating.AnimationKey, floating.AnimationTime);
    }

    private void OnEntParentChanged(EntityUid uid, FloatingVisualsComponent component, ref EntParentChangedMessage args)
    {
        var transform = args.Transform;
        if (CanFloat(uid, component, transform))
            FloatAnimation(uid, component.Offset, component.AnimationKey, component.AnimationTime);
    }
}