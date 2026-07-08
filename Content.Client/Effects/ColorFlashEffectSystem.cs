// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Effects;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;
using Robust.Shared.Collections;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client.Effects;

public sealed class ColorFlashEffectSystem : SharedColorFlashEffectSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;
    [Dependency] private readonly IComponentFactory _factory = default!; // EE Plasmamen Change
    [Dependency] private readonly SpriteSystem _sprite = default!;
    /// <summary>
    /// It's a little on the long side but given we use multiple colours denoting what happened it makes it easier to register.
    /// </summary>
    private const float AnimationLength = 0.30f;
    private const string AnimationKey = "color-flash-effect";
    private ValueList<EntityUid> _toRemove = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeAllEvent<ColorFlashEffectEvent>(OnColorFlashEffect);
        SubscribeLocalEvent<ColorFlashEffectComponent, AnimationCompletedEvent>(OnEffectAnimationCompleted);
    }

    // EE Plasmamen Change
    public override void RaiseEffect(Color color, List<EntityUid> entities, Filter filter, float? animationLength = null)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        OnColorFlashEffect(new ColorFlashEffectEvent(color, GetNetEntityList(entities), animationLength)); // EE Plasmamen Change
    }

    private void OnEffectAnimationCompleted(EntityUid uid, ColorFlashEffectComponent component, AnimationCompletedEvent args)
    {
        if (args.Key != AnimationKey)
            return;

        if (TryComp<SpriteComponent>(uid, out var sprite))
        {
            _sprite.SetColor((uid, sprite), component.Color);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = AllEntityQuery<ColorFlashEffectComponent>();
        _toRemove.Clear();

        // Can't use deferred removal on animation completion or it will cause issues.
        while (query.MoveNext(out var uid, out _))
        {
            if (_animation.HasRunningAnimation(uid, AnimationKey))
                continue;

            _toRemove.Add(uid);
        }

        foreach (var ent in _toRemove)
        {
            RemComp<ColorFlashEffectComponent>(ent);
        }
    }

    // EE Plasmamen Change
    private Animation? GetDamageAnimation(EntityUid uid, Color color, SpriteComponent? sprite = null, float? animationLength = null)
    {
        if (!Resolve(uid, ref sprite, false))
            return null;

        // 90% of them are going to be this so why allocate a new class.
        return new Animation
        {
            Length = TimeSpan.FromSeconds(animationLength ?? AnimationLength), // EE Plasmamen Change
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Color),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(color, 0f),
                        new AnimationTrackProperty.KeyFrame(sprite.Color, animationLength ?? AnimationLength) // EE Plasmamen Change
                    }
                }
            }
        };
    }

    private void OnColorFlashEffect(ColorFlashEffectEvent ev)
    {
        var color = ev.Color;

        foreach (var nent in ev.Entities)
        {
            var ent = GetEntity(nent);

            if (Deleted(ent) || !TryComp(ent, out SpriteComponent? sprite))
            {
                continue;
            }

            // EE Plasmamen Change Start
            if (!TryComp(ent, out AnimationPlayerComponent? player))
            {
                player = (AnimationPlayerComponent) _factory.GetComponent(typeof(AnimationPlayerComponent));
                player.Owner = ent;
                player.NetSyncEnabled = false;
                AddComp(ent, player);
            }

            // Need to stop the existing animation first to ensure the sprite color is fixed.
            // Otherwise we might lerp to a red colour instead.
            if (_animation.HasRunningAnimation(ent, player, AnimationKey))
                _animation.Stop(ent, player, AnimationKey);

            if (TryComp<ColorFlashEffectComponent>(ent, out var effect))
                sprite.Color = effect.Color;

            var animation = GetDamageAnimation(ent, color, sprite, ev.AnimationLength);

            if (animation == null)
                continue;
            // EE Plasmamen Change End

            if (!TryComp(ent, out ColorFlashEffectComponent? comp))
            {
#if DEBUG
                DebugTools.Assert(!_animation.HasRunningAnimation(ent, AnimationKey));
#endif
            }

            _animation.Stop(ent, AnimationKey);

            if (animation == null)
            {
                continue;
            }

            var targetEv = new GetFlashEffectTargetEvent(ent);
            RaiseLocalEvent(ent, ref targetEv);
            ent = targetEv.Target;

            EnsureComp<ColorFlashEffectComponent>(ent, out comp);
            comp.NetSyncEnabled = false;
            comp.Color = sprite.Color;

            _animation.Play(ent, animation, AnimationKey);
        }
    }
}

/// <summary>
/// Raised on an entity to change the target for a color flash effect.
/// </summary>
[ByRefEvent]
public record struct GetFlashEffectTargetEvent(EntityUid Target);