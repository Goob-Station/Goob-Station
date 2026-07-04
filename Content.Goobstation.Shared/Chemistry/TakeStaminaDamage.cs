// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SX-7 <92227810+SX-7@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.shared.Chemistry;

public sealed partial class TakeStaminaDamageSystem : EntityEffectSystem<StaminaComponent, TakeStaminaDamage>
{
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;

    protected override void Effect(Entity<StaminaComponent> entity, ref EntityEffectEvent<TakeStaminaDamage> args)
    {
        _stamina.TakeStaminaDamage(entity.Owner, args.Effect.Amount, visual: false, immediate: args.Effect.Immediate);
    }
}

[UsedImplicitly]
public sealed partial class TakeStaminaDamage : EntityEffectBase<TakeStaminaDamage>
{
    /// <summary>
    /// How much stamina damage to take.
    /// </summary>
    [DataField]
    public int Amount = 10;

    /// <summary>
    /// Whether stamina damage should be applied immediately
    /// </summary>
    [DataField]
    public bool Immediate;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-deal-stamina-damage",
            ("immediate", Immediate),
            ("amount", MathF.Abs(Amount)),
            ("chance", Probability),
            ("deltasign", MathF.Sign(Amount)));
}
