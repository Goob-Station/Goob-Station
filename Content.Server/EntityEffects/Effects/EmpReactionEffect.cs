// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2023 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 mhamster <81412348+mhamsterr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Emp;
using Content.Shared.EntityEffects;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects;


[DataDefinition]
public sealed partial class EmpReactionEffect : EntityEffect
{
    /// <summary>
    ///     Impulse range per unit of quantity
    /// </summary>
    [DataField("rangePerUnit")]
    public float EmpRangePerUnit = 0.5f;

    /// <summary>
    ///     Maximum impulse range
    /// </summary>
    [DataField("maxRange")]
    public float EmpMaxRange = 10;

    /// <summary>
    ///     How much energy will be drain from sources
    /// </summary>
    [DataField]
    public float EnergyConsumption = 12500;

    /// <summary>
    ///     Amount of time entities will be disabled
    /// </summary>
    [DataField("duration")]
    public float DisableDuration = 15;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
            => Loc.GetString("reagent-effect-guidebook-emp-reaction-effect", ("chance", Probability));

    public override void Effect(EntityEffectBaseArgs args)
    {
        var tSys = args.EntityManager.System<TransformSystem>();
        var transform = args.EntityManager.GetComponent<TransformComponent>(args.TargetEntity);

        var range = EmpRangePerUnit;

        if (args is EntityEffectReagentArgs reagentArgs)
        {
            range = MathF.Min((float) (reagentArgs.Quantity * EmpRangePerUnit), EmpMaxRange);
        }

        args.EntityManager.System<EmpSystem>()
            .EmpPulse(tSys.GetMapCoordinates(args.TargetEntity, xform: transform),
            range,
            EnergyConsumption,
            DisableDuration);
    }
}