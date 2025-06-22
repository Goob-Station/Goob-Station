using Content.Pirate.Shared.AddSpriteLayerAfter;
using Robust.Client.GameObjects;

namespace Content.Pirate.Client.AddSpriteLayerAfter;


public sealed class AddSpriteLayerAfterSystem : SharedAddSpriteLayerAfterSystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AddSpriteLayerAfterComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(EntityUid uid, AddSpriteLayerAfterComponent component, ref AppearanceChangeEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;
        if (component.LayerIndex == null) return;
        if (sprite.LayerExists(component.LayerIndex)) return;
        var newLayer = new PrototypeLayerData()
        {
            State = component.State,
            RsiPath = component.Sprite,
            Visible = component.Visible
        };
        var layerIndex = sprite.AddLayer(newLayer);
        sprite.LayerMapSet(component.LayerIndex, layerIndex);
    }
}
