// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.EntityConditions;
using Content.Shared.Mind;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitcode.Wizard.Chemistry;

public sealed partial class HasComponentConditionSystem : EntityConditionSystem<MetaDataComponent, HasComponentCondition>
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly EntityManager _ent = default!;

    protected override void Condition(Entity<MetaDataComponent> ent, ref EntityConditionEvent<HasComponentCondition> args)
    {
        var target = ent.Owner;

        bool entHasComp = args.Condition.ConsiderAll
            ? args.Condition.Components.Values.All(c => _ent.HasComponent(target, c.Component.GetType()))
            : args.Condition.Components.Values.Any(c => _ent.HasComponent(target, c.Component.GetType()));

        bool mindEntHasComp = false;
        if (args.Condition.CheckMind && _mind.TryGetMind(target, out var mindId, out _))
        {
            mindEntHasComp = args.Condition.ConsiderAll
                ? args.Condition.Components.Values.All(c => _ent.HasComponent(mindId, c.Component.GetType()))
                : args.Condition.Components.Values.Any(c => _ent.HasComponent(mindId, c.Component.GetType()));
        }

        var hasComp = entHasComp || mindEntHasComp;

        args.Result = hasComp ^ args.Condition.Inverted;
    }
}

/// <inheritdoc cref="EntityCondition"/>
[UsedImplicitly]
public sealed partial class HasComponentCondition : EntityConditionBase<HasComponentCondition>
{
    /// <summary>
    /// The set of components that this condition cares about
    /// </summary>
    [DataField(required: true)]
    public ComponentRegistry Components = default!;

    /// <summary>
    /// Whether the check is an existential or universal check
    /// </summary>
    [DataField]
    public bool ConsiderAll;

    /// <summary>
    /// Whether we check the mind entity for the components
    /// </summary>
    [DataField]
    public bool CheckMind;

    /// <summary>
    /// Guidebook text
    /// </summary>
    [DataField]
    public LocId? GuidebookComponentName;

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        if (GuidebookComponentName == null)
            return string.Empty;

        return Loc.GetString("reagent-effect-condition-guidebook-has-component",
            ("comp", Loc.GetString(GuidebookComponentName)),
            ("invert", Inverted));
    }
}
