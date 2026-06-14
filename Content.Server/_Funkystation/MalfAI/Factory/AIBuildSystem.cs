// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared._Funkystation.MalfAI;
using Content.Shared._Funkystation.MalfAI.Factory;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Map;

namespace Content.Server._Funkystation.MalfAI.Factory;

/// <summary>
/// Event to request building a prototype at a specific location.
/// </summary>
[ByRefEvent]
public readonly record struct AIBuildRequestEvent(EntityUid Requester, EntityCoordinates Target, string Prototype);

/// <summary>
/// Handles Malf AI building requests by spawning prototypes at specified locations after a DoAfter.
/// </summary>
public sealed class AIBuildSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedMalfAiFactorySystem _factory = default!;

    private static readonly TimeSpan BuildDelay = TimeSpan.FromSeconds(3);

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AIBuildRequestEvent>(OnBuildRequest);
        SubscribeLocalEvent<MalfAiMarkerComponent, AIBuildDoAfterEvent>(OnBuildDoAfter);
    }

    private void OnBuildRequest(ref AIBuildRequestEvent args)
    {
        if (!_factory.IsTileFree(args.Target, out var tileCenter))
            return;

        // Show the DoAfter on the AI's remote eye when possible (the brain is hidden in the core).
        var doAfterUser = args.Requester;
        var core = Transform(args.Requester).ParentUid;
        if (TryComp<StationAiCoreComponent>(core, out var coreComp) && coreComp.RemoteEntity is { } eye)
            doAfterUser = eye;

        var doAfterEvent = new AIBuildDoAfterEvent(GetNetCoordinates(tileCenter), args.Prototype);
        var doAfterArgs = new DoAfterArgs(EntityManager, doAfterUser, BuildDelay, doAfterEvent, eventTarget: args.Requester)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = false,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnBuildDoAfter(Entity<MalfAiMarkerComponent> ent, ref AIBuildDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        var location = GetCoordinates(args.Location);
        if (!_factory.IsTileFree(location, out var tileCenter))
            return;

        var spawned = Spawn(args.Prototype, tileCenter);

        // If this is a robotics factory, remember who built it so created borgs go to the right AI.
        if (HasComp<RoboticsFactoryGridComponent>(spawned))
        {
            var owner = EnsureComp<MalfFactoryOwnerComponent>(spawned);
            owner.Controller = ent.Owner;

            // The factory is single use: remove the build action.
            RemoveRoboticsFactoryAction(ent.Owner);
        }

        var xform = Transform(spawned);
        if (!xform.Anchored)
            _transform.AnchorEntity(spawned, xform);

        args.Handled = true;
    }

    private void RemoveRoboticsFactoryAction(EntityUid performer)
    {
        var toRemove = new List<EntityUid>();
        foreach (var action in _actions.GetActions(performer))
        {
            if (MetaData(action.Owner).EntityPrototype?.ID == "ActionMalfAiRoboticsFactory")
                toRemove.Add(action.Owner);
        }

        foreach (var id in toRemove)
            _actions.RemoveAction(performer, id);
    }
}
