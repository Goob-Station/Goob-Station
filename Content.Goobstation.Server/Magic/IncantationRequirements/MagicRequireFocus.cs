using Content.Goobstation.Shared.Magic;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;

namespace Content.Goobstation.Server.Magic.IncantationRequirements;

public sealed partial class MagicRequireFocus : IncantationRequirement
{
    [DataField(required: true)] public float Weight = 1f;

    public override bool Valid(EntityUid ent, IEntityManager entMan, out string reason)
    {
        var inventorySystem = entMan.System<InventorySystem>();
        var handsSystem = entMan.System<SharedHandsSystem>();

        var items = 0;

        // in case of an ascended heretic or such.
        // since they no longer need a focus, why not turn them into one? :D
        if (entMan.TryGetComponent<MagicFocusProviderComponent>(ent, out var pmic))
            items += pmic.Weight;

        var ise = inventorySystem.GetSlotEnumerator(ent, SlotFlags.WITHOUT_POCKET);
        while (ise.MoveNext(out var container))
        {
            var item = container.ContainedEntity;
            if (!item.HasValue || !entMan.TryGetComponent<MagicFocusProviderComponent>(item.Value, out var mic)) continue;
            items += mic.Weight;
        }

        var hands = handsSystem.EnumerateHeld(ent);
        foreach (var held in hands)
            items += entMan.TryGetComponent<MagicFocusProviderComponent>(held, out var mic) ? mic.Weight : 0;

        reason = Loc.GetString("magic-requirements-items", ("n", Weight - items));
        return items >= Weight;
    }
}
