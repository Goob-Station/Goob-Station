// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Reaction;
using Content.Shared.Database;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityEffects;

/// <summary>
///     Extinguishes nearby entities.
/// </summary>
public sealed partial class ExtinguishNearbySystem
    : EntityEffectSystem<ReactiveComponent, ExtinguishNearby>
{
    protected override void Effect(Entity<ReactiveComponent> entity, ref EntityEffectEvent<ExtinguishNearby> args)
    {
        var ev = new ExtinguishNearby(args.Effect.Range);
        EntityManager.EventBus.RaiseLocalEvent(entity.Owner, ev);
    }
}

public sealed partial class ExtinguishNearby : EntityEffectBase<ExtinguishNearby>
{
    [DataField]
    public float Range = 12;

    public ExtinguishNearby() { }

    public ExtinguishNearby(float range)
    {
        Range = range;
    }

    public override bool ShouldLog => true;

    public override LogImpact LogImpact => LogImpact.Medium;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-ignite", ("chance", Probability));
}
