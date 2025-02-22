using Content.Client.DamageState;
using Content.Shared._Goobstation.Blob;
using Content.Shared._Goobstation.Blob.Components;
using Robust.Client.GameObjects;

namespace Content.Client._Goobstation.Blob;

public sealed class BlobbernautSystem : SharedBlobbernautSystem
{

}

public sealed class BlobbernautVisualizerSystem : VisualizerSystem<BlobbernautComponent>
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BlobbernautComponent, AfterAutoHandleStateEvent>(OnBlobTileHandleState);
    }

    private static readonly DamageStateVisualLayers[] Layers =
    [
        DamageStateVisualLayers.Base, DamageStateVisualLayers.BaseUnshaded,
    ];

    private void UpdateAppearance(EntityUid id, BlobbernautComponent blobbernaut, AppearanceComponent? appearance = null, SpriteComponent? sprite = null)
    {
        if (!Resolve(id, ref appearance, ref sprite))
            return;

        foreach (var key in Layers)
        {
            if (!sprite.LayerMapTryGet(key, out _))
                continue;

            sprite.LayerSetColor(key, blobbernaut.Color);
        }
    }

    protected override void OnAppearanceChange(EntityUid uid, BlobbernautComponent component, ref AppearanceChangeEvent args)
    {
        UpdateAppearance(uid, component, args.Component, args.Sprite);
    }

    private void OnBlobTileHandleState(EntityUid uid, BlobbernautComponent component, ref AfterAutoHandleStateEvent args)
    {
        UpdateAppearance(uid, component);
    }
}
