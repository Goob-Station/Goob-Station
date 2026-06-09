using Content.Client.Clothing;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared.Clothing;
using Content.Shared.Item;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Slasher.Systems;

public sealed class SpringlockVisualizerSystem : VisualizerSystem<SpringlockClothingComponent>
{
    [Dependency] private readonly SharedItemSystem _itemSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpringlockClothingComponent, GetEquipmentVisualsEvent>(OnGetEquipmentVisuals,
            after: [typeof(ClientClothingSystem)]);
    }

    protected override void OnAppearanceChange(EntityUid uid, SpringlockClothingComponent component, ref AppearanceChangeEvent args)
    {
        _itemSystem.VisualsChanged(uid);
    }

    private void OnGetEquipmentVisuals(Entity<SpringlockClothingComponent> ent, ref GetEquipmentVisualsEvent args)
    {
        if (!ent.Comp.IsLocked
            || !ent.Comp.LockedClothingVisuals.TryGetValue(args.Slot, out var layers))
            return;

        var slotPrefix = $"{args.Slot}-";
        for (var i = args.Layers.Count - 1; i >= 0; i--)
        {
            if (args.Layers[i].Item1.StartsWith(slotPrefix))
                args.Layers.RemoveAt(i);
        }

        foreach (var layer in layers)
            args.Layers.Add(($"{args.Slot}-locked", layer));
    }
}
