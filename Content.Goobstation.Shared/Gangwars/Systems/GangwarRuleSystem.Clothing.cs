using Content.Goobstation.Shared.Gangwars.Components;

namespace Content.Goobstation.Shared.Gangwars.Systems;

public sealed partial class GangwarRuleSystem
{
    /// <summary>
    /// Returns the number of gang clothes they are currently wearing.
    /// </summary>
    public int CountGangClothingSlots(EntityUid entity, Color? gangColor = null)
    {
        var slots = new[] { "shoes", "outerClothing", "jumpsuit", "head" };
        var count = 0;
        foreach (var slot in slots)
        {
            if (!_inventory.TryGetSlotEntity(entity, slot, out var item)
                || !TryComp<GangClothingComponent>(item, out var clothing)
                || gangColor.HasValue && clothing.Gang != gangColor.Value)
                continue;

            count++;
        }
        return count;
    }

    /// <summary>
    /// Returns true if the entity has at least 3 gang clothing pieces equipped,
    /// Purposely only requires 3 pieces due to onis not wearing shoes.
    /// </summary>
    public bool IsWearingGangOutfit(EntityUid entity, Color? gangColor = null) =>
        CountGangClothingSlots(entity, gangColor) >= 3;
}
