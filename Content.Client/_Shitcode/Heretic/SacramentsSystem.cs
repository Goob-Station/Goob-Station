using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Systems;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;

namespace Content.Client._Shitcode.Heretic;

public sealed class SacramentsSystem : SharedSacramentsSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;

    private const string AnimationKey = "eye_flash";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SacramentsOfPowerComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SacramentsOfPowerComponent, AppearanceChangeEvent>(OnAppearanceChange);
        SubscribeLocalEvent<SacramentsOfPowerComponent, AnimationCompletedEvent>(OnAnimation);
        SubscribeNetworkEvent<SacramentsPulseEvent>(OnPulseEvent);
    }

    private void OnAnimation(Entity<SacramentsOfPowerComponent> ent, ref AnimationCompletedEvent args)
    {
        if (args.Key != AnimationKey || !TryComp(ent, out SpriteComponent? sprite))
            return;

        _appearance.OnChangeData(ent, sprite);
    }

    private void OnShutdown(Entity<SacramentsOfPowerComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        if (!_sprite.LayerMapTryGet((ent, sprite), SacramentsKey.Key, out var layer, false))
            return;

        _animation.Stop(ent.Owner, AnimationKey);
        _sprite.RemoveLayer((ent, sprite), layer);
    }

    private void OnAppearanceChange(Entity<SacramentsOfPowerComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null || !args.AppearanceData.TryGetValue(SacramentsKey.Key, out var state))
            return;

        _animation.Stop(ent.Owner, AnimationKey);
        var sprite = new SpriteSpecifier.Rsi(ent.Comp.SpritePath, ent.Comp.SpriteStates[(SacramentsState) state]);

        if (!_sprite.LayerMapTryGet((ent, args.Sprite), SacramentsKey.Key, out var layer, false))
        {
            layer = _sprite.AddLayer((ent, args.Sprite), sprite);
            args.Sprite.LayerSetShader(layer, "unshaded");
            _sprite.LayerMapSet((ent, args.Sprite), SacramentsKey.Key, layer);
        }
        else
            _sprite.LayerSetSprite((ent, args.Sprite), layer, sprite);

        _sprite.LayerSetAutoAnimated((ent, args.Sprite), layer, true);
    }

    private void OnPulseEvent(SacramentsPulseEvent ev)
    {
        PlayPulseAnimation(GetEntity(ev.Entity));
    }

    private void PlayPulseAnimation(EntityUid uid)
    {
        if (_animation.HasRunningAnimation(uid, AnimationKey))
            return;

        var a = GetAnimation();
        _animation.Play(uid, a, AnimationKey);
    }

    private static Animation GetAnimation()
    {
        return new Animation
        {
            Length = TimeSpan.FromMilliseconds(400),
            AnimationTracks =
            {
                new AnimationTrackSpriteFlick
                {
                    LayerKey = SacramentsKey.Key,
                    KeyFrames =
                    {
                        new AnimationTrackSpriteFlick.KeyFrame("eye_flash", 0f)
                    }
                }
            }
        };
    }
}
