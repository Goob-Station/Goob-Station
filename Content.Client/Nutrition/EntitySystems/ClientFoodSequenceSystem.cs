using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;
using Robust.Shared.Prototypes; // Goobstation - anythingburgers

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
            if (state.Sprite is null && state.Proto != null && _prototypeManager.TryIndex<EntityPrototype>(state.Proto, out var prototype)) // Goobstation - anythingburgers
            {
                prototype.TryGetComponent<SpriteComponent>(out var spriteComp);
                if (spriteComp != null)
                {
                    var rsiPath = spriteComp.BaseRSI?.Path.ToString();
                    var rsiState = spriteComp.LayerGetState(0).ToString();

                    if (rsiPath != null && rsiState != null)
                    {
                        state.Sprite = new SpriteSpecifier.Rsi(new ResPath(rsiPath), rsiState);
                    }
                }
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
