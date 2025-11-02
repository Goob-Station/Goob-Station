using Content.Goobstation.Shared.Ranching.Food;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Ranching.Food;

public sealed class FoodSeedVisualsSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FoodSeedVisualsComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(Entity<FoodSeedVisualsComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!_appearance.TryGetData<Color>(ent.Owner, SeedColor.Color, out var color)
            || !_sprite.LayerMapTryGet((ent.Owner, args.Sprite), SeedColor.Color, out var layer, false))
            return;

        _sprite.LayerSetColor(ent.Owner, layer, color);
    }
}
