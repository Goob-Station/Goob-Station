using Content.Shared._CorvaxGoob.ColorVisuals;
using Robust.Client.GameObjects;

namespace Content.Client._CorvaxGoob.ColorVisuals;

public sealed class ColorVisualsSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ColorVisualsComponent, AppearanceChangeEvent>(OnChangeData);
        SubscribeLocalEvent<ColorVisualsComponent, ComponentShutdown>(OnComponentShutdown);
    }

    private void OnChangeData(Entity<ColorVisualsComponent> entity, ref AppearanceChangeEvent args)
    {
        if (!args.AppearanceData.TryGetValue(Shared._CorvaxGoob.ColorVisuals.ColorVisuals.Color, out var color) ||
            args.Sprite == null) return;

        _sprite.SetColor((entity.Owner, args.Sprite), (Color) color);
    }

    private void OnComponentShutdown(Entity<ColorVisualsComponent> entity, ref ComponentShutdown args)
    {
        if (TryComp<SpriteComponent>(entity, out var sprite))
            _sprite.SetColor((entity.Owner, sprite), Color.White);
    }
}
