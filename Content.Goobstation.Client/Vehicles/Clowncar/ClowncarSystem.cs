// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Vehicles.Clowncar;
using Robust.Client.Animations;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Vehicles.Clowncar;

public sealed class ClowncarSystem : SharedClowncarSystem
{
    [Dependency] private readonly AnimationPlayerSystem _animationPlayer = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ClowncarComponent, AppearanceChangeEvent>(OnAppearanceChange);
        SubscribeLocalEvent<ClowncarComponent, AnimationCompletedEvent>(OnAnimationCompleted);
    }

    private void OnAppearanceChange(EntityUid uid, ClowncarComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null || !AppearanceSystem.TryGetData<bool>(uid, ClowncarVisuals.FireModeEnabled, out var fireModeEnabled, args.Component))
            return;

        if (!_sprite.LayerMapTryGet((uid, args.Sprite), ClowncarLayers.Base, out var baseLayerIdx, true))
            return;

        var state = _sprite.LayerGetRsiState((uid, args.Sprite), baseLayerIdx); // unused....
    }

    private void OnAnimationCompleted(EntityUid uid, ClowncarComponent component, AnimationCompletedEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        _sprite.LayerSetAutoAnimated((uid, sprite),ClowncarLayers.Base, false);
    }

    private void PlayAnimation(EntityUid uid, ClowncarLayers layer, string state, string finalState, float animationTime)
    {
        if (_animationPlayer.HasRunningAnimation(uid, state))
            return;

        var animation = new Animation()
        {
            Length = TimeSpan.FromSeconds(animationTime),
            AnimationTracks =
            {
                new AnimationTrackSpriteFlick
                {
                    LayerKey = layer,
                    KeyFrames =
                    {
                        new AnimationTrackSpriteFlick.KeyFrame(state, 0f),
                        new AnimationTrackSpriteFlick.KeyFrame(finalState, animationTime)
                    }
                }
            }
        };

        _animationPlayer.Play(uid, animation, state);
    }
}

internal enum ClowncarLayers : byte
{
   Base
}
