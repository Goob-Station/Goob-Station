// SPDX-FileCopyrightText: 2024 Centronias <me@centronias.com>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SX-7 <92227810+SX-7@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.EntityConditions;
using Content.Shared.EntityConditions.Conditions;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.EntityEffects.EffectConditions;

public sealed partial class StaminaDamageThresholdSystem : EntityConditionSystem<StaminaComponent, StaminaDamageThreshold>
{
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;

    protected override void Condition(Entity<StaminaComponent> entity, ref EntityConditionEvent<StaminaDamageThreshold> args)
    {
        var total = _stamina.GetStaminaDamage(entity.Owner, entity.Comp);

        args.Result = total > args.Condition.Min && total < args.Condition.Max;
    }
}

public sealed partial class StaminaDamageThreshold : EntityConditionBase<StaminaDamageThreshold>
{
    [DataField]
    public float Max = float.PositiveInfinity;

    [DataField]
    public float Min = -1;

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return Loc.GetString("reagent-effect-condition-guidebook-stamina-damage-threshold",
            ("max", float.IsPositiveInfinity(Max) ? (float) int.MaxValue : Max),
            ("min", Min));
    }
}
