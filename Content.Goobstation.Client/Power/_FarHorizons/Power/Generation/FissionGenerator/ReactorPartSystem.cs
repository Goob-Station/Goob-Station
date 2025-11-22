using Content.Goobstation.Shared.Power._FarHorizons.Power.Generation.FissionGenerator;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Power._FarHorizons.Power.Generation.FissionGenerator;

public sealed class ReactorPartSystem : SharedReactorPartSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<Shared.Power._FarHorizons.Power.Generation.FissionGenerator.ReactorPartComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(EntityUid uid, Shared.Power._FarHorizons.Power.Generation.FissionGenerator.ReactorPartComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        // Re-enable if/when there are multiple sprites
        //if (!_sprite.LayerMapTryGet((uid, args.Sprite), ReactorCapVisualLayers.Sprite, out var layer, false))
        //    return;

        _sprite.LayerSetColor((uid, args.Sprite), 0, _proto.Index(component.Material).Color);
    }

    protected override void AccUpdate()
    {
        var query = EntityQueryEnumerator<Shared.Power._FarHorizons.Power.Generation.FissionGenerator.ReactorPartComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            _sprite.LayerSetColor((uid, EntityManager.GetComponent<SpriteComponent>(uid)), 0, _proto.Index(component.Material).Color);
        }
    }
}
