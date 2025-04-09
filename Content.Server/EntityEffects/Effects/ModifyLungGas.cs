// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Body.Components;
using Content.Shared.Atmos;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects;

public sealed partial class ModifyLungGas : EntityEffect
{
    [DataField("ratios", required: true)]
    private Dictionary<Gas, float> _ratios = default!;

    // JUSTIFICATION: This is internal magic that players never directly interact with.
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;

    public override void Effect(EntityEffectBaseArgs args)
    {

        LungComponent? lung;
        float amount = 1f;

        if (args is EntityEffectReagentArgs reagentArgs)
        {
            if (!args.EntityManager.TryGetComponent<LungComponent>(reagentArgs.OrganEntity, out var organLung))
                return;
            lung = organLung;
            amount = reagentArgs.Quantity.Float();
        }
        else
        {
            if (!args.EntityManager.TryGetComponent<LungComponent>(args.TargetEntity, out var organLung)) //Likely needs to be modified to ensure it works correctly
                return;
            lung = organLung;
        }

        if (lung != null)
        {
            foreach (var (gas, ratio) in _ratios)
            {
                var quantity = ratio * amount / Atmospherics.BreathMolesToReagentMultiplier;
                if (quantity < 0)
                    quantity = Math.Max(quantity, -lung.Air[(int) gas]);
                lung.Air.AdjustMoles(gas, quantity);
            }
        }
    }
}