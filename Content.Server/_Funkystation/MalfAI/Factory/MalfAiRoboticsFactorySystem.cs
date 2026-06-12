// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Shared._Funkystation.MalfAI;
using Content.Shared._Funkystation.MalfAI.Actions;

namespace Content.Server._Funkystation.MalfAI.Factory;

/// <summary>
/// Handles the Robotics Factory action by requesting a RoboticsFactoryGrid build at the target.
/// </summary>
public sealed class MalfAiRoboticsFactorySystem : EntitySystem
{
    private const string RoboticsFactoryPrototype = "RoboticsFactoryGrid";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MalfAiMarkerComponent, MalfAiRoboticsFactoryActionEvent>(OnRoboticsFactory);
    }

    private void OnRoboticsFactory(Entity<MalfAiMarkerComponent> ent, ref MalfAiRoboticsFactoryActionEvent args)
    {
        if (args.Handled)
            return;

        if (!args.Target.IsValid(EntityManager))
            return;

        // The server decides the prototype: the client cannot specify it.
        var buildRequest = new AIBuildRequestEvent(ent.Owner, args.Target, RoboticsFactoryPrototype);
        RaiseLocalEvent(buildRequest);

        args.Handled = true;
    }
}
