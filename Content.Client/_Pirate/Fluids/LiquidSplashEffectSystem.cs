using Content.Shared._Pirate.Fluids;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Client._Pirate.Fluids;

public sealed class LiquidSplashEffectSystem : EntitySystem
{
    private const string SplashLayerKey = "PirateLiquidSplash";
    private static readonly SpriteSpecifier.Rsi SplashSprite = new(new ResPath("_Pirate/Effects/liquid_splash.rsi"), "splash");

    [Dependency] private readonly SpriteSystem _sprite = default!;

    private readonly Dictionary<EntityUid, int> _generations = [];

    public override void Initialize()
    {
        SubscribeNetworkEvent<LiquidSplashEffectEvent>(OnLiquidSplashed);
    }

    private void OnLiquidSplashed(LiquidSplashEffectEvent args)
    {
        var target = GetEntity(args.Target);
        if (!TryComp<SpriteComponent>(target, out var sprite))
            return;

        if (_sprite.LayerMapTryGet((target, sprite), SplashLayerKey, out var existingLayer, false))
            _sprite.RemoveLayer((target, sprite), existingLayer);

        var layer = _sprite.AddLayer((target, sprite), SplashSprite);
        _sprite.LayerMapSet((target, sprite), SplashLayerKey, layer);
        _sprite.LayerSetColor((target, sprite), layer, args.Color);
        sprite.LayerSetShader(layer, "unshaded");

        var generation = _generations.GetValueOrDefault(target) + 1;
        _generations[target] = generation;
        Timer.Spawn(TimeSpan.FromSeconds(1), () => RemoveSplash(target, generation));
    }

    private void RemoveSplash(EntityUid target, int generation)
    {
        if (!_generations.TryGetValue(target, out var activeGeneration) ||
            activeGeneration != generation ||
            !TryComp<SpriteComponent>(target, out var sprite) ||
            !_sprite.LayerMapTryGet((target, sprite), SplashLayerKey, out var layer, false))
        {
            return;
        }

        _generations.Remove(target);
        _sprite.RemoveLayer((target, sprite), layer);
    }
}
