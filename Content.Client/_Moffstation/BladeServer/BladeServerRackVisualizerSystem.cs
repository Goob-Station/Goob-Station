using System.Linq;
using Content.Shared._Moffstation.BladeServer;
using Robust.Client.GameObjects;

namespace Content.Client._Moffstation.BladeServer;

/// <summary>
/// This is a <see cref="VisualizerSystem{T}"/> for entities with <see cref="BladeServerRackComponent"/>. It manages
/// dynamic sprite layers for blade servers inserted into server racks.
/// </summary>
public sealed partial class BladeServerRackVisualizerSystem : VisualizerSystem<BladeServerRackComponent>
{
    protected override void OnAppearanceChange(
        EntityUid uid,
        BladeServerRackComponent component,
        ref AppearanceChangeEvent args
    )
    {
        if (args.Sprite is not { } sprite)
            return;

        var entity = new Entity<SpriteComponent?, AppearanceComponent>(uid, sprite, args.Component);
        var addedLayerIndices = EnsureComp<BladeServerRackVisualsComponent>(entity).AddedLayerKeys;

        // It's easiest to just obliterate all old layers and re-add all layers needed, rather than trying to figure out
        // what's changed and updating layers based on that.
        foreach (var addedLayerIndex in addedLayerIndices)
        {
            SpriteSystem.RemoveLayer(entity, addedLayerIndex);
        }

        addedLayerIndices.Clear();

        if (!AppearanceSystem.TryGetData<BladeServerRackSlotVisualData.Group>(
                entity,
                BladeServerRackVisuals.SlotsKey,
                out var slots,
                entity
            ))
            return;

        foreach (var (idx, slot) in slots.Slots.Index())
        {
            if (slot is null)
                // Slot is empty.
                continue;

            var baseLayer = SpriteSystem.AddRsiLayer(entity, "blade-base");
            SpriteSystem.LayerSetOffset(entity, baseLayer, slot.Offset);
            var key = $"blade-base-{idx}";
            SpriteSystem.LayerMapSet(entity, key, baseLayer);
            addedLayerIndices.Add(key);

            if (slot.StripeColor is { } color)
            {
                var stripeLayer = SpriteSystem.AddRsiLayer(entity, "blade-stripe");
                SpriteSystem.LayerSetColor(entity, stripeLayer, color);
                SpriteSystem.LayerSetOffset(entity, stripeLayer, slot.Offset);
                var stripeKey = $"blade-stripe-{idx}";
                SpriteSystem.LayerMapSet(entity, stripeKey, stripeLayer);
                addedLayerIndices.Add(stripeKey);
            }

            if (slot.Powered)
            {
                var poweredLayer = SpriteSystem.AddRsiLayer(entity, $"blade-powered-{idx}");
                SpriteSystem.LayerSetOffset(entity, poweredLayer, slot.Offset);
                sprite.LayerSetShader(poweredLayer, SpriteSystem.UnshadedId);
                var poweredKey = $"blade-powered-{idx}";
                SpriteSystem.LayerMapSet(entity, poweredKey, poweredLayer);
                addedLayerIndices.Add(poweredKey);
            }
        }
    }
}

/// <summary>
/// This component is just used to track dynamically added sprite layers for <see name="BladeServerRackComponent"/>.
/// </summary>
[RegisterComponent, Access(typeof(BladeServerRackVisualizerSystem))]
public sealed partial class BladeServerRackVisualsComponent : Component
{
    [ViewVariables]
    public readonly List<string> AddedLayerKeys = [];
}
