// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Database;
using Content.Shared.EntityEffects;
using Content.Shared.Humanoid;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityEffects;

/// <summary>
///     Scrambles the dna of nearby humanoids.
/// </summary>
public sealed partial class ScrambleNearbyEffectSystem : EntityEffectSystem<HumanoidAppearanceComponent, ScrambleNearbyEffect>
{
    protected override void Effect(Entity<HumanoidAppearanceComponent> entity, ref EntityEffectEvent<ScrambleNearbyEffect> args)
    {
        var ev = new ScrambleNearbyEffect(args.Effect.Radius);
        EntityManager.EventBus.RaiseLocalEvent(entity.Owner, ev);
    }
}

public sealed partial class ScrambleNearbyEffect : EntityEffectBase<ScrambleNearbyEffect>
{
    [DataField] public float Radius = 7;

    public ScrambleNearbyEffect() { }

    public ScrambleNearbyEffect(float radius)
    {
        Radius = radius;
    }

    public override bool ShouldLog => true;

    public override LogImpact LogImpact => LogImpact.Medium;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-scramble-nearby");
}
