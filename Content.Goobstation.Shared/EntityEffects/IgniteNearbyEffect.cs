// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
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
///     Ignites mobs nearby.
/// </summary>
public sealed partial class IgniteNearbyEffectSystem
    : EntityEffectSystem<ReactiveComponent, IgniteNearbyEffect>
{
    protected override void Effect(Entity<ReactiveComponent> entity, ref EntityEffectEvent<IgniteNearbyEffect> args)
    {
        var ev = new IgniteNearbyEffect(args.Effect.Radius, args.Effect.FireStacks);
        EntityManager.EventBus.RaiseLocalEvent(entity.Owner, ev);
    }
}

public sealed partial class IgniteNearbyEffect : EntityEffectBase<IgniteNearbyEffect>
{
    [DataField] public float Radius = 7;

    [DataField] public float FireStacks = 2;

    public IgniteNearbyEffect(float radius, float fireStacks)
    {
        Radius = radius;
        FireStacks = fireStacks;
    }

    public override bool ShouldLog => true;

    public override LogImpact LogImpact => LogImpact.Medium;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-ignite", ("chance", Probability));
}
