// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using System.Numerics;
using Content.Shared.Gravity;
using Robust.Client.GameObjects;
using Robust.Client.Animations;
using Robust.Shared.Animations;

namespace Content.Client.Gravity;

/// <inheritdoc/>
public sealed class FloatingVisualizerSystem : SharedFloatingVisualizerSystem
{
    [Dependency] private readonly AnimationPlayerSystem AnimationSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FloatingVisualsComponent, AnimationCompletedEvent>(OnAnimationCompleted);
    }

    /// <inheritdoc/>
    public override void FloatAnimation(EntityUid uid, Vector2 offset, string animationKey, float animationTime, bool stop = false)
    {
        if (stop)
        {
            AnimationSystem.Stop(uid, animationKey);
            return;
        }

        var animation = new Animation
        {
            // We multiply by the number of extra keyframes to make time for them
            Length = TimeSpan.FromSeconds(animationTime*2),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0f),
                        new AnimationTrackProperty.KeyFrame(offset, animationTime),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, animationTime),
                    }
                }
            }
        };

        if (!AnimationSystem.HasRunningAnimation(uid, animationKey))
            AnimationSystem.Play(uid, animation, animationKey);
    }

    private void OnAnimationCompleted(EntityUid uid, FloatingVisualsComponent component, AnimationCompletedEvent args)
    {
        if (args.Key != component.AnimationKey)
            return;

        FloatAnimation(uid, component.Offset, component.AnimationKey, component.AnimationTime, stop: !component.CanFloat);
    }
}