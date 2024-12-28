using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes; // Goobstation - anythingburgers
using Robust.Shared.Utility;

namespace Content.Client.Nutrition.EntitySystems;

public sealed class ClientFoodSequenceSystem : SharedFoodSequenceSystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<FoodSequenceStartPointComponent, AfterAutoHandleStateEvent>(OnHandleState);
    }

    private void OnHandleState(Entity<FoodSequenceStartPointComponent> start, ref AfterAutoHandleStateEvent args)
    {
        if (!TryComp<SpriteComponent>(start, out var sprite))
            return;

        UpdateFoodVisuals(start, sprite);
    }

    private void UpdateFoodVisuals(Entity<FoodSequenceStartPointComponent> start, SpriteComponent? sprite = null)
    {
        if (!Resolve(start, ref sprite, false))
            return;

        //Remove old layers
        foreach (var key in start.Comp.RevealedLayers)
        {
            sprite.RemoveLayer(key);
        }
        start.Comp.RevealedLayers.Clear();

        //Add new layers
        var counter = 0;
        foreach (var state in start.Comp.FoodLayers)
        {
            if (state.Sprite is null && state.Proto != null && _prototypeManager.TryIndex<EntityPrototype>(state.Proto, out var prototype)) // Goobstation - anythingburgers HOLY FUCK THIS IS SO BAD!!! BUT IT WORKS!!
            {
                if (prototype.TryGetComponent<SpriteComponent>(out var spriteComp))
                {
                    var rsiPath = spriteComp.BaseRSI?.Path.ToString();
                    if (rsiPath == null)
                        continue;
                    var layercount = 0;
                    foreach (var layer in spriteComp.AllLayers)
                    {
                        if (!layer.RsiState.IsValid || !layer.Visible || layer.ActualRsi == null || layer.RsiState == null || layer.RsiState.Name == null)
                            continue;

                        state.Sprite = new SpriteSpecifier.Rsi(layer.ActualRsi.Path, layer.RsiState.Name);

                        var keyCodeProto = $"food-layer-{counter}-{layer.RsiState.Name}-{layercount}";
                        layercount++;
                        start.Comp.RevealedLayers.Add(keyCodeProto);

                        sprite.LayerMapTryGet(start.Comp.TargetLayerMap, out var indexProto);

                        if (start.Comp.InverseLayers)
                            indexProto++;

                        sprite.AddBlankLayer(indexProto);
                        sprite.LayerMapSet(keyCodeProto, indexProto);
                        sprite.LayerSetSprite(indexProto, state.Sprite);
                        sprite.LayerSetColor(indexProto, layer.Color);

                        var layerPosProto = start.Comp.StartPosition;
                        layerPosProto += (start.Comp.Offset * counter) + state.LocalOffset;
                        sprite.LayerSetOffset(indexProto, layerPosProto);

                    }
                }
                counter++;
                continue;
            }


            if (state.Sprite is null)
                continue;

            var keyCode = $"food-layer-{counter}";
            start.Comp.RevealedLayers.Add(keyCode);

            sprite.LayerMapTryGet(start.Comp.TargetLayerMap, out var index);

            if (start.Comp.InverseLayers)
                index++;

            sprite.AddBlankLayer(index);
            sprite.LayerMapSet(keyCode, index);
            sprite.LayerSetSprite(index, state.Sprite);

            //Offset the layer
            var layerPos = start.Comp.StartPosition;
            layerPos += (start.Comp.Offset * counter) + state.LocalOffset;
            sprite.LayerSetOffset(index, layerPos);

            counter++;
        }
    }
}
