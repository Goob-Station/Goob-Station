using Content.Goobstation.Shared.Power._FarHorizons.Power.Generation.FissionGenerator;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Power._FarHorizons.Power.Generation.FissionGenerator;

public sealed class ReactorPartVisualSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<Shared.Power._FarHorizons.Power.Generation.FissionGenerator.ReactorPartVisualComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(EntityUid uid, Shared.Power._FarHorizons.Power.Generation.FissionGenerator.ReactorPartVisualComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!_sprite.LayerMapTryGet((uid, args.Sprite), ReactorCapVisualLayers.Sprite, out var layer, false))
            return;

        _sprite.LayerSetColor((uid, args.Sprite), layer, comp.color);
    }
}
