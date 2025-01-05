using System.Linq;
using Content.Shared.Actions;
using Content.Shared.Mind;
using Content.Shared.Store;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Wizard.LesserSummonGuns;

public sealed partial class BuyerActionsDontHaveTagCondition : ListingCondition
{
    [DataField(required: true)]
    public ProtoId<TagPrototype> Tag;

    public override bool Condition(ListingConditionArgs args)
    {
        var tagSystem = args.EntityManager.System<TagSystem>();
        var mindSystem = args.EntityManager.System<SharedMindSystem>();
        return !mindSystem.TryGetMind(args.Buyer, out var mindId, out _) ||
               !args.EntityManager.TryGetComponent(mindId, out ActionsContainerComponent? container) ||
               container.Container.ContainedEntities.All(e => !tagSystem.HasTag(e, Tag));
    }
}
