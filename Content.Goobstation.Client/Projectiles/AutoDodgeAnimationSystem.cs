using System.Numerics;
using Content.Goobstation.Shared.Projectiles;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;

namespace Content.Goobstation.Client.Projectiles;

/// <summary>
/// Plays the dodge animation on auto-dodging entities.
/// </summary>
public sealed class AutoDodgeAnimationSystem : EntitySystem
{
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<AutoDodgeEffectEvent>(OnDodgeEffect);
    }

    private void OnDodgeEffect(AutoDodgeEffectEvent ev)
    {
        if (!TryGetEntity(ev.Entity, out var uid))
            return;

        if (!HasComp<SpriteComponent>(uid) || !TryComp<AutoDodgeComponent>(uid, out var dodge))
            return;

        _animation.Stop(uid.Value, dodge.AnimationKey);
        _animation.Play(uid.Value, GetWeaveAnimation(ev.Direction, dodge), dodge.AnimationKey);
    }

    /// <summary>
    /// A quick sideways weave.
    /// </summary>
    private static Animation GetWeaveAnimation(Vector2 direction, AutoDodgeComponent dodge)
    {
        var length = (float) dodge.AnimationLength.TotalSeconds;
        var offset = direction * dodge.SidestepDistance;
        var lean = Angle.FromDegrees(-dodge.LeanDegrees * MathF.Sign(direction.X));

        return new Animation
        {
            Length = dodge.AnimationLength,
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Cubic,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0f),
                        new AnimationTrackProperty.KeyFrame(offset, length * 0.2f),
                        new AnimationTrackProperty.KeyFrame(offset, length * 0.285f),
                        new AnimationTrackProperty.KeyFrame(-offset * 0.12f, length * 0.315f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, length * 0.2f),
                    }
                },
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Rotation),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Angle.Zero, 0f),
                        new AnimationTrackProperty.KeyFrame(lean, length * 0.23f),
                        new AnimationTrackProperty.KeyFrame(lean, length * 0.255f),
                        new AnimationTrackProperty.KeyFrame(new Angle(-lean.Theta * 0.18), length * 0.315f),
                        new AnimationTrackProperty.KeyFrame(Angle.Zero, length * 0.2f),
                    }
                },
            }
        };
    }
}
