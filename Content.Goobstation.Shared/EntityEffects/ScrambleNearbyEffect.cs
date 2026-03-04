// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Database;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityEffects;

/// <summary>
///     Scrambles the dna of nearby humanoids.
/// </summary>
public sealed partial class ScrambleNearbyEffect : EventEntityEffect<ScrambleNearbyEffect>
{

    [DataField] public float Radius = 7;

    public ScrambleNearbyEffect(float radius)
    {
        Radius = radius;
    }

    public override bool ShouldLog => true;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-scramble-nearby");

    public override LogImpact LogImpact => LogImpact.Medium;

    public override void Effect(EntityEffectBaseArgs args)
    {
        var ev = new ScrambleNearbyEffect(Radius);
        args.EntityManager.EventBus.RaiseLocalEvent(args.TargetEntity, ev);
    }
}
