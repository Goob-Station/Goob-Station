// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Wizard.Chemistry;

[UsedImplicitly]
public sealed partial class HasComponentCondition : EntityEffectCondition
{
    [DataField(required: true)]
    public HashSet<string> Components = new();

    [DataField(required: true)]
    public string GuidebookComponentName = default!;

    [DataField]
    public bool Invert;

    public override bool Condition(EntityEffectBaseArgs args)
    {
        var hasComp = false;
        foreach (var component in Components)
        {
            hasComp = args.EntityManager.HasComponent(args.TargetEntity,
                args.EntityManager.ComponentFactory.GetRegistration(component).Type);

            if (hasComp)
                break;
        }

        return hasComp ^ Invert;
    }

    public override string GuidebookExplanation(IPrototypeManager prototype)
    {
        return Loc.GetString("reagent-effect-condition-guidebook-has-component",
            ("comp", Loc.GetString(GuidebookComponentName)),
            ("invert", Invert));
    }
}