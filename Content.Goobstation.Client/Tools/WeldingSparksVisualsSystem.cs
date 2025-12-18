using Content.Goobstation.Shared.Tools;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;
using System.Numerics;

namespace Content.Goobstation.Client.Tools;

public sealed class WeldingSparksVisualsSystem : EntitySystem
{
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;

    private const string ANIM_KEY = "WeldDoor";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<PlayWeldAnimationEvent>(PlayWeldAnimation);
    }

    private void PlayWeldAnimation(PlayWeldAnimationEvent ev)
    {
        var ent = GetEntity(ev.SparksEnt);
        var (weldDir, alreadyWelded, duration) = ev.AnimData;

        if (!HasComp<WeldingSparksVisualsComponent>(ent))
            return;

        var animationPlayer = EnsureComp<AnimationPlayerComponent>(ent);
        if (_animation.HasRunningAnimation(ent, animationPlayer, ANIM_KEY))
            return;

        // `0.5f` is halfway between the centre of the source tile and the centre of the next tile over, so right on the edge.
        var startingOffset = weldDir switch
        {
            AnimationDir.Vertical => new Vector2(0f, 0.5f), // Airlocks
            AnimationDir.Horizontal => new Vector2(-0.5f, 0f), // Firelocks
            _ => throw new Exception($"Invalid welding sparks `AnimationDir` argument: '{weldDir}'")
        };

        // Go in reverse if unwelding.
        if (alreadyWelded)
        {
            startingOffset = -startingOffset;
        }

        var animation = new Animation()
        {
            Length = duration,
            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        // Move from one side of the tile to the other.
                        new AnimationTrackProperty.KeyFrame(startingOffset, 0f),
                        new AnimationTrackProperty.KeyFrame(-startingOffset, (float) duration.TotalSeconds),
                    }
                }
            }
        };

        _animation.Play(ent, animation, ANIM_KEY);
    }
}
