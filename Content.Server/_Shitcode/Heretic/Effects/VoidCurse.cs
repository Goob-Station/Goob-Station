// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Goobstation.Heretic.EntitySystems.PathSpecific;
using Content.Shared.EntityEffects;
using Content.Shared.Humanoid;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Heretic.Effects;

public sealed partial class VoidCurseSystemEffect : EntityEffectSystem<HumanoidAppearanceComponent, VoidCurse> // HumanoidAppearanceComponent here is ass but i cba
{
    [Dependency] private readonly VoidCurseSystem _voidCurse = default!;

    protected override void Effect(Entity<HumanoidAppearanceComponent> entity, ref EntityEffectEvent<VoidCurse> args)
    {
        _voidCurse.DoCurse(entity.Owner, args.Effect.Stacks);
    }
}

public sealed partial class VoidCurse : EntityEffectBase<VoidCurse>
{
    [DataField]
    public int Stacks = 1;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => "Inflicts void curse.";
}
