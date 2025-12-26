using Content.Client.Humanoid;
using Content.Goobstation.Shared.SecondSkin;
using Content.Shared.Humanoid;
using Robust.Client.GameObjects;
using Robust.Shared.Containers;

namespace Content.Goobstation.Client.SecondSkin;

public sealed class SecondSkinSystem : SharedSecondSkinSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SecondSkinHolderComponent, AppearanceChangeEvent>(OnAppearanceChange);
        SubscribeLocalEvent<SecondSkinHolderComponent, ComponentStartup>(OnStartup);
    }

    protected override void UpdateSprite(Entity<HumanoidAppearanceComponent> ent)
    {
        if (!TryComp(ent, out SpriteComponent? sprite))
            return;

        _humanoid.UpdateSprite((ent, ent.Comp, sprite));
    }

    private void OnStartup(Entity<SecondSkinHolderComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.Container = Container.EnsureContainer<ContainerSlot>(ent, ent.Comp.ContainerId);
    }

    private void OnAppearanceChange(Entity<SecondSkinHolderComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        var layer = EnsureLayer((ent.Owner, ent.Comp, args.Sprite));

        if (Appearance.TryGetData<bool>(ent, SecondSkinKey.Equipped, out var equipped, args.Component))
            _sprite.LayerSetVisible((ent, args.Sprite), layer, equipped);

        if (Appearance.TryGetData<Color>(ent, SecondSkinKey.Color, out var color, args.Component))
            _sprite.LayerSetColor((ent, args.Sprite), layer, color);
    }

    private int EnsureLayer(Entity<SecondSkinHolderComponent, SpriteComponent> ent)
    {
        if (_sprite.LayerMapTryGet((ent, ent.Comp2), SecondSkinKey.Key, out var layer, false))
            return layer;

        layer = _sprite.AddLayer((ent, ent.Comp2), ent.Comp1.Sprite);
        _sprite.LayerSetVisible((ent, ent.Comp2), layer, false);
        _sprite.LayerMapSet((ent, ent.Comp2), SecondSkinKey.Key, layer);
        return layer;
    }
}
