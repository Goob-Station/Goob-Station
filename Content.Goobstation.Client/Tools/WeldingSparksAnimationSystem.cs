using Content.Goobstation.Shared.Tools;
using Content.Shared.Tools.Components;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Animations;
using System.Numerics;

namespace Content.Goobstation.Client.Tools;

public sealed class WeldingSparksAnimationSystem : EntitySystem
{
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;

    private const string ANIM_KEY = "WeldAnim";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<SpawnedWeldingSparksEvent>(OnSpawnedWeldingSparks);
    }

    private void OnSpawnedWeldingSparks(SpawnedWeldingSparksEvent ev)
    {
        var targetEnt = GetEntity(ev.TargetEnt);
        if (!TryComp<WeldableComponent>(targetEnt, out var weldableComp) || !TryComp<WeldingSparksAnimationComponent>(targetEnt, out var sparksAnim))
            return;

        if (!TryGetEntity(ev.SparksEnt, out var sparksEnt))
            // `targetEnt` is validated with the `TryComp()` calls, so that can just use `GetEntity()`.
            return;

        var animationPlayer = EnsureComp<AnimationPlayerComponent>(targetEnt);
        if (_animation.HasRunningAnimation(targetEnt, animationPlayer, ANIM_KEY))
            return;

        var (startOffset, endOffset) = GetOffsets((targetEnt, sparksAnim), weldableComp.IsWelded);

        var animation = new Animation()
        {
            Length = ev.Duration,
            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(startOffset, 0f),
                        new AnimationTrackProperty.KeyFrame(endOffset, (float) ev.Duration.TotalSeconds),
                    }
                }
            }
        };

        _animation.Play(sparksEnt.Value, animation, ANIM_KEY);
    }

    private (Vector2, Vector2) GetOffsets(Entity<WeldingSparksAnimationComponent> ent, bool isWelded)
    {
        var start = ent.Comp.StartingOffset;
        // If there's no manual `EndingOffset`, just go to the opposite of `StartingOffset`.
        var end = ent.Comp.EndingOffset ?? -ent.Comp.StartingOffset;

        // Rotation
        // Honestly I don't understand all of RT's sprite/eye/world/cardinal rotation stuff. I just trial-and-error'd this into working.
        // (why isn't there a helper function for this) :(
        if (TryComp<SpriteComponent>(ent, out var sprite))
        {
            var worldRotation = _transformSystem.GetWorldRotation(ent);
            var eyeRotation = _eyeManager.CurrentEye.Rotation;

            var relativeRotation = (worldRotation + eyeRotation).Reduced().FlipPositive();

            var cardinalSnapping = sprite.SnapCardinals ? relativeRotation.GetCardinalDir().ToAngle() : Angle.Zero;

            var finalAngle = sprite.NoRotation ? relativeRotation : relativeRotation - cardinalSnapping;

            start = finalAngle.RotateVec(start); // `RotateVec()` contains a `Theta == 0` check, so no need to check for it in here.
            end = finalAngle.RotateVec(end);
        }

        // Welding.
        if (!isWelded)
        {
            return (start, end);
        }
        // Unwelding. (go backwards)
        else
        {
            return (end, start);
        }
    }
}
