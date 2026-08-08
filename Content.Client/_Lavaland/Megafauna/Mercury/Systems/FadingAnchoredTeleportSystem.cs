using Content.Shared._Lavaland.Megafauna.Mercury.Components;
using Content.Shared._Lavaland.Megafauna.Mercury.Systems;
using Robust.Client.Animations;
using Robust.Client.GameObjects;

namespace Content.Client._Lavaland.Megafauna.Mercury.Systems;

/// <summary>
/// Basically just FadinngTimedDespawn innit
/// </summary>
public sealed class FadingAnchoredTeleportSystem : SharedFadingAnchoredTeleportSystem
{
    [Dependency] private readonly AnimationPlayerSystem _animationPlayer = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    protected override void FadeOut(Entity<FadingAnchoredTeleportComponent> ent)
    {
        var (uid, comp) = ent;

        if (!TryComp(uid, out SpriteComponent? sprite))
            return;

        _animationPlayer.Stop(uid, FadingAnchoredTeleportComponent.AnimationKey);
        _sprite.SetColor((uid, sprite), sprite.Color.WithAlpha(1f));

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
                        new AnimationTrackProperty.KeyFrame(sprite.Color.WithAlpha(1f), 0f),
                        new AnimationTrackProperty.KeyFrame(sprite.Color.WithAlpha(0f), comp.FadeOutTime),
                    },
                },
            },
        };

        _animationPlayer.Play(uid, animation, FadingAnchoredTeleportComponent.AnimationKey);
    }
    protected override void FadeIn(Entity<FadingAnchoredTeleportComponent> ent)
    {
        var (uid, comp) = ent;

        if (!TryComp(uid, out SpriteComponent? sprite))
            return;

        _animationPlayer.Stop(uid, FadingAnchoredTeleportComponent.AnimationKey);
        _sprite.SetColor((uid, sprite), sprite.Color.WithAlpha(0f));

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
                        new AnimationTrackProperty.KeyFrame(sprite.Color.WithAlpha(0f), 0f),
                        new AnimationTrackProperty.KeyFrame(sprite.Color.WithAlpha(1f), comp.FadeOutTime),
                    },
                },
            },
        };

        _animationPlayer.Play(uid, animation, FadingAnchoredTeleportComponent.AnimationKey);
    }
}
