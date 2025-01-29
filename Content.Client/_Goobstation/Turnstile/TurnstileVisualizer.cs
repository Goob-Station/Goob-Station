using Content.Shared._Goobstation.Turnstile;
using Robust.Client.GameObjects;
using Robust.Client.Animations;
using Robust.Shared.Timing;

namespace Content.Client._Goobstation.Turnstile;

/// <summary>
/// Visualizer for the fax machine which displays the correct sprite based on the inserted entity.
/// </summary>
public sealed class TurnstileVisualizer : EntitySystem
{
    [Dependency] private readonly AnimationPlayerSystem _player = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TurnstileComponent, AppearanceChangeEvent>(OnAppearanceChanged);
    }

    private void OnAppearanceChanged(EntityUid uid, TurnstileComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (_player.HasRunningAnimation(uid, "turnstileOperate") || _player.HasRunningAnimation(uid, "turnstileDeny"))
            return;

        if (_appearance.TryGetData(uid, TurnstileVisuals.State, out TurnstileVisualState visuals))
        {
            switch (visuals)
            {
                case TurnstileVisualState.Allow:
                    _player.Play(uid,
                        new Animation()
                        {
                            Length = TimeSpan.FromSeconds(1),
                            AnimationTracks =
                            {
                                new AnimationTrackSpriteFlick()
                                {
                                    LayerKey = TurnstileVisuals.State,
                                    KeyFrames =
                                    {
                                        new AnimationTrackSpriteFlick.KeyFrame(component.OpeningSpriteState, 0f),
                                        new AnimationTrackSpriteFlick.KeyFrame("turnstile", 2.4f),

                                    },
                                },
                            },
                        },
                        "turnstileOperate");

                    resetSprite(uid);
                    break;
                case TurnstileVisualState.Deny:
                    _player.Play(uid,
                        new Animation()
                        {
                            Length = TimeSpan.FromSeconds(1),
                            AnimationTracks =
                            {
                                new AnimationTrackSpriteFlick()
                                {
                                    LayerKey = TurnstileVisuals.State,
                                    KeyFrames =
                                    {
                                        new AnimationTrackSpriteFlick.KeyFrame(component.DenySpriteState, 0f),
                                        new AnimationTrackSpriteFlick.KeyFrame("turnstile", 2.4f),
                                    },
                                },
                            },
                        },
                        "turnstileDeny");
                    resetSprite(uid);
                    break;
            }
        }
    }

    private void resetSprite(EntityUid uid)
    {
        Timer.Spawn(1000, () =>
        {
            if(!TryComp<SpriteComponent>(uid, out var spriteComponent))
                return;
            spriteComponent.LayerSetState(0,"turnstile");
        });
    }
}
