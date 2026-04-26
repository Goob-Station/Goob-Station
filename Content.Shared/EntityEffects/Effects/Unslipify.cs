// SPDX-FileCopyrightText: 2024 drakewill-CRL <46307022+drakewill-CRL@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Physics;
using Content.Shared.Slippery;
using Content.Shared.StepTrigger.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared.EntityEffects.Effects;

/// <summary>
/// Goobstation - Makes slippery mob not slippery anymore.
/// Use this when an entity is given slipify effect and you want to remove it
/// Todo - Remove this when you find better idea
/// </summary>
public sealed partial class Unslipify : EntityEffect
{
    public override void Effect(EntityEffectBaseArgs args)
    {
        var fixtureSystem = args.EntityManager.System<FixtureSystem>();
        var colWakeSystem = args.EntityManager.System<CollisionWakeSystem>();

        args.EntityManager.RemoveComponentDeferred<SlipperyComponent>(args.TargetEntity);
        args.EntityManager.RemoveComponentDeferred<StepTriggerComponent>(args.TargetEntity);

        if (args.EntityManager.TryGetComponent<FixturesComponent>(args.TargetEntity, out var fixtures))
        {
            if (fixtures.Fixtures.ContainsKey("slips"))
            {
                fixtureSystem.DestroyFixture(args.TargetEntity, "slips", manager: fixtures);
            }
        }

        if (args.EntityManager.TryGetComponent<CollisionWakeComponent>(args.TargetEntity, out var collisionWake))
        {
            colWakeSystem.SetEnabled(args.TargetEntity, true, collisionWake);
        }
    }

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        throw new NotImplementedException();
    }
}
