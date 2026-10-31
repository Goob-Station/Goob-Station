using Content.Shared.Clothing.Components;
using Robust.Shared.Containers;

namespace Content.Shared.Clothing.EntitySystems;

public sealed partial class HideLayerClothingSystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public void SetWearerLayersHidden(EntityUid clothing, bool hide)
    {
        if (!TryComp<HideLayerClothingComponent>(clothing, out var hideComp)
            || !TryComp<ClothingComponent>(clothing, out var clothingComp)
            || clothingComp.InSlot == null
            || !_container.TryGetContainingContainer((clothing, null, null), out var container))
            return;

        SetLayerVisibility((clothing, hideComp, clothingComp), container.Owner, hide);
    }
}
