// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 pathetic meowmeow <uhhadd@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Trigger;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Audio.Systems;

namespace Content.Client.Trigger;

public sealed class TimerTriggerVisualizerSystem : VisualizerSystem<TimerTriggerVisualsComponent>
{
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TimerTriggerVisualsComponent, ComponentInit>(OnComponentInit);
    }

    private void OnComponentInit(EntityUid uid, TimerTriggerVisualsComponent comp, ComponentInit args)
    {
        comp.PrimingAnimation = new Animation {
            Length = TimeSpan.MaxValue,
            AnimationTracks = {
                new AnimationTrackSpriteFlick() {
                    LayerKey = TriggerVisualLayers.Base,
                    KeyFrames = { new AnimationTrackSpriteFlick.KeyFrame(comp.PrimingSprite, 0f) }
                }
            },
        };

        if (comp.PrimingSound != null)
        {
            comp.PrimingAnimation.AnimationTracks.Add(
                new AnimationTrackPlaySound() {
                    KeyFrames = { new AnimationTrackPlaySound.KeyFrame(_audioSystem.ResolveSound(comp.PrimingSound), 0) }
                }
            );
        }
    }

    protected override void OnAppearanceChange(EntityUid uid, TimerTriggerVisualsComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null
        ||  !TryComp<AnimationPlayerComponent>(uid, out var animPlayer))
            return;

        if (!AppearanceSystem.TryGetData<TriggerVisualState>(uid, TriggerVisuals.VisualState, out var state, args.Component))
            state = TriggerVisualState.Unprimed;

        switch (state)
        {
            case TriggerVisualState.Primed:
                if (!AnimationSystem.HasRunningAnimation(uid, animPlayer, TimerTriggerVisualsComponent.AnimationKey))
                    AnimationSystem.Play(uid, animPlayer, comp.PrimingAnimation, TimerTriggerVisualsComponent.AnimationKey);
                break;
            case TriggerVisualState.Unprimed:
                args.Sprite.LayerSetState(TriggerVisualLayers.Base, comp.UnprimedSprite);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public enum TriggerVisualLayers : byte
{
    Base
}