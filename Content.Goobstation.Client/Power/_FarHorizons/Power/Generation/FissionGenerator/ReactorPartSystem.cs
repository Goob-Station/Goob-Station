using Content.Shared._FarHorizons.Power.Generation.FissionGenerator;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Client._FarHorizons.Power.Generation.FissionGenerator;

public sealed class ReactorPartSystem : SharedReactorPartSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ReactorPartComponent, AppearanceChangeEvent>(OnAppearanceChange);
        SubscribeLocalEvent<ReactorPartComponent, ComponentInit>(OnComponentInit);
    }

    private void OnAppearanceChange(EntityUid uid, ReactorPartComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        // Re-enable if/when there are multiple sprites
        //if (!_sprite.LayerMapTryGet((uid, args.Sprite), ReactorCapVisualLayers.Sprite, out var layer, false))
        //    return;

        _sprite.LayerSetColor((uid, args.Sprite), 0, _proto.Index(component.Material).Color);
    }

    private void OnComponentInit(Entity<ReactorPartComponent> ent, ref ComponentInit args)
        => _sprite.LayerSetColor((ent.Owner, EntityManager.GetComponent<SpriteComponent>(ent.Owner)), 0, _proto.Index(ent.Comp.Material).Color);
}