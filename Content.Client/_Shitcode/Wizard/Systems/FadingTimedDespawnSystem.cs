using Content.Shared._Goobstation.Wizard.FadingTimedDespawn;
using Robust.Client.Animations;
using Robust.Client.GameObjects;

namespace Content.Client._Goobstation.Wizard.Systems;

public sealed class FadingTimedDespawnSystem : SharedFadingTimedDespawnSystem
{
    [Dependency] private readonly AnimationPlayerSystem _animationSystem = default!;

    protected override void FadeOut(Entity<FadingTimedDespawnComponent> ent)
    {
        base.FadeOut(ent);

        var (uid, comp) = ent;

        if (_animationSystem.HasRunningAnimation(uid, FadingTimedDespawnComponent.AnimationKey))
            return;

        if (!TryComp(uid, out SpriteComponent? sprite))
            return;

        var animation = new Animation
        {
            Length = TimeSpan.FromSeconds(comp.FadeOutTime),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Color),
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(sprite.Color, 0f),
                        new AnimationTrackProperty.KeyFrame(sprite.Color.WithAlpha(0f), comp.FadeOutTime),
                    },
                },
            },
        };

        _animationSystem.Play(uid, animation, FadingTimedDespawnComponent.AnimationKey);
    }

    protected override bool CanDelete(EntityUid uid)
    {
        return IsClientSide(uid);
    }
}
