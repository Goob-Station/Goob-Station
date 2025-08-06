// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Sprinting;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Sprinting;

public sealed class SprintingSystem : SharedSprintingSystem
{
    [Dependency] private readonly AnimationPlayerSystem _animationPlayer = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private static readonly Animation InitialAnimation = new()
    {
        Length = TimeSpan.FromSeconds(0.48),
        AnimationTracks =
        {
            new AnimationTrackSpriteFlick
            {
                LayerKey = SprintVisualLayers.Base,
                KeyFrames =
                {
                    new AnimationTrackSpriteFlick.KeyFrame(new RSI.StateId("sprint_cloud"), 0f),
                }
            }
        }
    };

    private static readonly Animation SmallAnimation = new()
    {
        Length = TimeSpan.FromSeconds(0.34),
        AnimationTracks =
        {
            new AnimationTrackSpriteFlick
            {
                LayerKey = SprintVisualLayers.Base,
                KeyFrames =
                {
                    new AnimationTrackSpriteFlick.KeyFrame(new RSI.StateId("sprint_cloud_small"), 0f),
                },
            },
        },
    };
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SprinterComponent, SprintStartEvent>(OnSprintStart);
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SprinterComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (!component.IsSprinting
                || !_timing.IsFirstTimePredicted
                || _timing.CurTime - component.LastStep < component.TimeBetweenSteps)
                continue;

            var ent = Spawn(component.StepAnimation, Transform(uid).Coordinates);
            _animationPlayer.Play(ent, SmallAnimation, "sprint_cloud_small");
            component.LastStep = _timing.CurTime;
        }
    }
    private void OnSprintStart(EntityUid uid, SprinterComponent component, ref SprintStartEvent args)
    {
        if (TerminatingOrDeleted(uid)
            || !_timing.IsFirstTimePredicted)
            return;

        var ent = Spawn(component.SprintAnimation, Transform(uid).Coordinates);
        _animationPlayer.Play(ent, InitialAnimation, "sprint_cloud");
    }

    public enum SprintVisualLayers : byte
    {
        Base,
    }

}
