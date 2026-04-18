// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Mind;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Content.Shared.EntityConditions;

namespace Content.Shared._Shitcode.Wizard.Chemistry;

public sealed partial class HasComponentConditionSystem : EntityConditionSystem<MetaDataComponent, HasComponentCondition>
{
    [Dependency] private readonly SharedMindSystem _mind = default!;

    protected override void Condition(Entity<MetaDataComponent> entity, ref EntityConditionEvent<HasComponentCondition> args) // Metadata comp fucking kill me holy shit.
    {
        EntityUid? mind = null;
        if (args.Condition.CheckMind && _mind.TryGetMind(entity.Owner, out var mindId, out _))
            mind = mindId;

        var hasComp = false;
        foreach (var component in args.Condition.Components)
        {
            var comp = EntityManager.ComponentFactory.GetRegistration(component).Type;
            hasComp = EntityManager.HasComponent(entity.Owner, comp) ||
                      EntityManager.HasComponent(mind, comp);

            if (hasComp)
                break;
        }

        args.Result = hasComp ^ args.Condition.Invert;
    }
}

/// <inheritdoc cref="EntityCondition"/>
[UsedImplicitly]
public sealed partial class HasComponentCondition : EntityConditionBase<HasComponentCondition>
{
    [DataField(required: true)]
    public HashSet<string> Components = new();

    [DataField]
    public LocId? GuidebookComponentName;

    [DataField]
    public bool Invert;

    [DataField]
    public bool CheckMind;

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        if (GuidebookComponentName == null)
            return string.Empty;

        return Loc.GetString("reagent-effect-condition-guidebook-has-component",
            ("comp", Loc.GetString(GuidebookComponentName)),
            ("invert", Invert));
    }
}
