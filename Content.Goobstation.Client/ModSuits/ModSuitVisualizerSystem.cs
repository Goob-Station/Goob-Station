using System.Linq;
using Content.Client.Clothing;
using Content.Goobstation.Shared.ModSuits.Components;
using Content.Shared.Clothing;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.ModSuits;

public sealed partial class ModSuitVisualizerSystem : VisualizerSystem<ModSuitSealedVisualsComponent>
{
    [Dependency] private SharedItemSystem _item = default!;

    [SubscribeLocalEvent(after: new[] { typeof(ClientClothingSystem) })]
    private void OnGetEquipmentVisuals(Entity<ModSuitSealedVisualsComponent> ent, ref GetEquipmentVisualsEvent args)
    {
        if (!TryComp(ent, out AppearanceComponent? appearance)
            || !AppearanceSystem.TryGetData<bool>(ent, ModSuitVisuals.Sealed, out var isSealed, appearance)
            || !isSealed)
            return;

        if (!ent.Comp.ClothingVisuals.TryGetValue(args.Slot, out var layers))
            return;

        if (TryComp(args.Equipee, out InventoryComponent? inventory)
            && inventory.SpeciesId != null
            && ent.Comp.ClothingVisuals.TryGetValue($"{args.Slot}-{inventory.SpeciesId}", out var speciesLayers))
            layers = speciesLayers;

        var i = 0;
        foreach (var layer in layers)
        {
            var key = layer.MapKeys?.FirstOrDefault();
            if (key == null)
            {
                key = i == 0 ? $"{args.Slot}-sealed" : $"{args.Slot}-sealed-{i}";
                i++;
            }

            args.Layers.Add((key, layer));
        }
    }

    protected override void OnAppearanceChange(EntityUid uid, ModSuitSealedVisualsComponent component, ref AppearanceChangeEvent args)
    {
        if (!AppearanceSystem.TryGetData<bool>(uid, ModSuitVisuals.Sealed, out var isSealed, args.Component))
            return;

        if (args.Sprite != null && SpriteSystem.LayerMapTryGet((uid, args.Sprite), component.SpriteLayer, out var layer, false))
            SpriteSystem.LayerSetVisible((uid, args.Sprite), layer, isSealed);

        _item.VisualsChanged(uid);
    }
}
