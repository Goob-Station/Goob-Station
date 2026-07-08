// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Content.Shared.Mind;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Wizard.Chemistry;

[UsedImplicitly]
public sealed partial class HasComponentCondition : EntityEffectCondition
{
    [DataField(required: true)]
    public HashSet<string> Components = new();

    [DataField]
    public LocId? GuidebookComponentName;

    [DataField]
    public bool Invert;

    [DataField]
    public bool CheckMind;

    public override bool Condition(EntityEffectBaseArgs args)
    {
        EntityUid? mind = null;
        if (CheckMind && args.EntityManager.System<SharedMindSystem>().TryGetMind(args.TargetEntity, out var mindId, out _))
            mind = mindId;

        var hasComp = false;
        foreach (var component in Components)
        {
            var comp = args.EntityManager.ComponentFactory.GetRegistration(component).Type;
            hasComp = args.EntityManager.HasComponent(args.TargetEntity, comp) ||
                      args.EntityManager.HasComponent(mind, comp);

            if (hasComp)
                break;
        }

        return hasComp ^ Invert;
    }

    public override string GuidebookExplanation(IPrototypeManager prototype)
    {
        if (GuidebookComponentName == null)
            return string.Empty;

        return Loc.GetString("reagent-effect-condition-guidebook-has-component",
            ("comp", Loc.GetString(GuidebookComponentName)),
            ("invert", Invert));
    }
}
