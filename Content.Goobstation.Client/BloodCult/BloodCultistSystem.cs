using System.Numerics;
using Content.Goobstation.Shared.BloodCult;
using Robust.Client.GameObjects;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.BloodCult;

public sealed class BloodCultistSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<Shared.BloodCult.Components.PentagramComponent, ComponentStartup>(OnPentagramAdded);
        SubscribeLocalEvent<Shared.BloodCult.Components.PentagramComponent, ComponentShutdown>(OnPentagramRemoved);
    }

    private void OnPentagramAdded(EntityUid uid, Shared.BloodCult.Components.PentagramComponent component, ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite) ||
            _sprite.LayerMapTryGet((uid, sprite), PentagramKey.Key, out _, false))
            return;

        var adj = _sprite.GetLocalBounds((uid, sprite)).Height / 2 + 1.0f / 32 * 10.0f;

        var randomState = _random.Pick(component.States);

        var layer = _sprite.AddLayer((uid, sprite), new SpriteSpecifier.Rsi(component.RsiPath, randomState));

        _sprite.LayerMapSet((uid, sprite), PentagramKey.Key, layer);
        _sprite.LayerSetOffset((uid, sprite), layer, new Vector2(0.0f, adj));
    }

    private void OnPentagramRemoved(EntityUid uid, Shared.BloodCult.Components.PentagramComponent component, ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite) ||
            !_sprite.LayerMapTryGet((uid, sprite), PentagramKey.Key, out var layer, false))
            return;

        _sprite.RemoveLayer((uid, sprite), layer);
    }
}
