using Content.Shared._CorvaxNext.MedipenRefiller;
using Robust.Client.GameObjects;

namespace Content.Client._CorvaxNext.MedipenRefiller;

public sealed class MedipenSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MedipenComponent, AppearanceChangeEvent>(OnAppearance);
    }

    private void OnAppearance(Entity<MedipenComponent> entity, ref AppearanceChangeEvent args)
    {
        if (args.Sprite is null)
            return;

        if (_appearance.TryGetData(entity, MedipenColorLayer.Fill, out var fillColor) &&
            _sprite.TryGetLayer((entity.Owner, args.Sprite), MedipenColorLayer.Fill, out var fillLayer, false))
        {
            if (fillColor is Color)
                fillLayer.Color = (Color)fillColor;
        }

        if (_appearance.TryGetData(entity, MedipenColorLayer.Empty, out var emptyColor) &&
            _sprite.TryGetLayer((entity.Owner, args.Sprite), MedipenColorLayer.Empty, out var emptyLayer, false))
        {
            if (emptyColor is Color)
                emptyLayer.Color = (Color)emptyColor;
        }
    }
}
