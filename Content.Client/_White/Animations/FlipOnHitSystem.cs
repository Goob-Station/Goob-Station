using Content.Shared._White.Animations;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;
using Robust.Shared.Timing;

namespace Content.Client._White.Animations;

public sealed class FlipOnHitSystem : SharedFlipOnHitSystem
{
    [Dependency] private readonly AnimationPlayerSystem _animationSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FlippingComponent, AnimationCompletedEvent>(OnAnimationComplete);
        SubscribeAllEvent<FlipOnHitEvent>(ev => PlayAnimation(GetEntity(ev.User)));
    }

    private void OnAnimationComplete(Entity<FlippingComponent> ent, ref AnimationCompletedEvent args)
    {
        if (args.Key != FlippingComponent.AnimationKey)
            return;

        PlayAnimation(ent);
    }

    protected override void PlayAnimation(EntityUid user)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        if (TerminatingOrDeleted(user))
            return;

        if (_animationSystem.HasRunningAnimation(user, FlippingComponent.AnimationKey))
        {
            EnsureComp<FlippingComponent>(user);
            return;
        }

        RemComp<FlippingComponent>(user);

        var baseAngle = Angle.Zero;
        if (EntityManager.TryGetComponent(user, out SpriteComponent? sprite))
            baseAngle = sprite.Rotation;

        var degrees = baseAngle.Degrees;

        var animation = new Animation
        {
            Length = TimeSpan.FromMilliseconds(1600),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Rotation),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(degrees - 10), 0f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(degrees + 180), 0.2f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(degrees + 360), 0.2f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(degrees + 540), 0.2f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(degrees + 720), 0.2f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(degrees + 900), 0.2f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(degrees + 1080), 0.2f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(degrees + 1260), 0.2f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(degrees + 1440), 0.2f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(degrees), 0f)
                    }
                }
            }
        };

        _animationSystem.Play(user, animation, FlippingComponent.AnimationKey);
    }
}
