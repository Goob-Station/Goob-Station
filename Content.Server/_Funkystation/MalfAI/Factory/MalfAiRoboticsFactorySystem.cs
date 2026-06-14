// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Shared._Funkystation.MalfAI;
using Content.Shared._Funkystation.MalfAI.Actions;
using Content.Shared._Funkystation.MalfAI.Factory;
using Robust.Shared.Network;

namespace Content.Server._Funkystation.MalfAI.Factory;

/// <summary>
/// Handles the Robotics Factory ghost: receives client placement confirmation and raises a local build request.
/// </summary>
public sealed class MalfAiRoboticsFactorySystem : EntitySystem
{
    private const string RoboticsFactoryPrototype = "RoboticsFactoryGrid";

    public override void Initialize()
    {
        base.Initialize();
        // Acknowledge the instant action so cooldown/handled is set correctly.
        SubscribeLocalEvent<MalfAiMarkerComponent, MalfAiRoboticsFactoryActionEvent>(OnRoboticsFactory);
        // Actual build request comes from the client ghost placement.
        SubscribeNetworkEvent<MalfAiFactoryBuildNetEvent>(OnFactoryBuildNet);
    }

    private void OnRoboticsFactory(Entity<MalfAiMarkerComponent> ent, ref MalfAiRoboticsFactoryActionEvent args)
    {
        args.Handled = true;
    }

    private void OnFactoryBuildNet(MalfAiFactoryBuildNetEvent msg, EntitySessionEventArgs args)
    {
        var session = args.SenderSession;

        // Validate that the sender actually controls the claimed performer entity.
        if (!TryGetEntity(msg.Performer, out var performer))
            return;

        if (session.AttachedEntity != performer)
            return;

        if (!HasComp<MalfAiMarkerComponent>(performer))
            return;

        var target = GetCoordinates(msg.Target);
        if (!target.IsValid(EntityManager))
            return;

        var buildRequest = new AIBuildRequestEvent(performer.Value, target, RoboticsFactoryPrototype);
        RaiseLocalEvent(ref buildRequest);
    }
}
