// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.EntitySystems;
using Content.Shared.Database;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.EntityEffects;

/// <summary>
///     Changes the temperature of the entity's local atmos mixture.
/// </summary>
public sealed partial class ChangeTempNearby : EntityEffect
{
    /// <summary>
    ///     Room temperature.
    /// </summary>
    [DataField]
    public float NewKelvin = 293.15f;

    public override bool ShouldLog => true;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-ignite", ("chance", Probability)); // why the fuck are these required, remind me to do these for all my new entity effects.

    public override LogImpact LogImpact => LogImpact.Medium;

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (args is not EntityEffectReagentArgs reagentArgs)
            return;

        var atmosSys = args.EntityManager.System<AtmosphereSystem>();

        var localAtmos = atmosSys.GetContainingMixture(args.TargetEntity);

        if (localAtmos != null)
            localAtmos.Temperature = NewKelvin;
    }
}
