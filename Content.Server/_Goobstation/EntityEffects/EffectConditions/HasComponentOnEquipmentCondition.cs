using Content.Shared.EntityEffects;
using Content.Shared.Inventory;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.EntityEffects.EffectConditions;

public sealed partial class HasComponentOnEquipmentCondition : EntityEffectCondition
{
    [DataField(required: true)]
    public ComponentRegistry Components = default!;

    [DataField]
    public bool Invert = false;

    public override bool Condition(EntityEffectBaseArgs args)
    {
        if (Components.Count == 0)
            return Invert;

        if (args.EntityManager.TryGetComponent<InventoryComponent>(args.TargetEntity, out var inv))
            if (args.EntityManager.System<InventorySystem>().TryGetContainerSlotEnumerator(args.TargetEntity, out var containerSlotEnumerator, SlotFlags.WITHOUT_POCKET))
                while (containerSlotEnumerator.NextItem(out var item))
                    foreach (var comp in Components)
                        if (args.EntityManager.HasComponent(item, comp.Value.Component.GetType()))
                            return !Invert;

        return Invert;
    }

    public override string GuidebookExplanation(IPrototypeManager prototype)
    {
        // This condition should be only used on reactive things and not on metabolisable chemicals
        // since equipment doesn't affect your internal metabolism. Additionally, components don't have
        // user-friendly, or even user-understandable names.
        return "TODO";
    }
}
