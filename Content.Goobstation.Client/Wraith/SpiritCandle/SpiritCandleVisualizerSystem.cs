using Content.Goobstation.Shared.Wraith.SpiritCandle;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Wraith.SpiritCandle;

public sealed class SpiritCandleVisualizerSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpiritCandleComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(Entity<SpiritCandleComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null
            || !_sprite.LayerMapTryGet((ent.Owner, args.Sprite), SpiritCandleVisuals.Layer, out var layer, false)
            || !_appearance.TryGetData<int>(ent.Owner, SpiritCandleVisuals.Layer, out var layerData))
            return;

        // this is a very unique item so its probably fine to hardcode the charges like this
        switch (layerData)
        {
            case 0:
                _sprite.LayerSetVisible((ent.Owner, args.Sprite), layer, false);
                break;
            case 1:
                _sprite.LayerSetVisible((ent.Owner, args.Sprite), layer, true);
                _sprite.LayerSetRsiState((ent.Owner, args.Sprite), layer, ent.Comp.OneCharge);
                break;
            case 2:
                _sprite.LayerSetVisible((ent.Owner, args.Sprite), layer, true);
                _sprite.LayerSetRsiState((ent.Owner, args.Sprite), layer, ent.Comp.TwoCharge);
                break;
        }
    }
}
