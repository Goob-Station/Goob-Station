// SPDX-FileCopyrightText: 2024 CaasGit <87243814+CaasGit@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 brainfood1183 <113240905+brainfood1183@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.GameObjects;
using Content.Shared.Fax.Components;
using Content.Shared.Fax;
using Robust.Client.Animations;

namespace Content.Client.Fax.System;

/// <summary>
/// Visualizer for the fax machine which displays the correct sprite based on the inserted entity.
/// </summary>
public sealed class FaxVisualsSystem : EntitySystem
{
    [Dependency] private readonly AnimationPlayerSystem _player = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FaxMachineComponent, AppearanceChangeEvent>(OnAppearanceChanged);
    }

    private void OnAppearanceChanged(EntityUid uid, FaxMachineComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (_player.HasRunningAnimation(uid, "faxecute"))
            return;

        if (_appearance.TryGetData(uid, FaxMachineVisuals.VisualState, out FaxMachineVisualState visuals) &&
            visuals == FaxMachineVisualState.Inserting)
        {
            _player.Play(uid,
                new Animation()
                {
                    Length = TimeSpan.FromSeconds(2.4),
                    AnimationTracks =
                    {
                        new AnimationTrackSpriteFlick()
                        {
                            LayerKey = FaxMachineVisuals.VisualState,
                            KeyFrames =
                            {
                                new AnimationTrackSpriteFlick.KeyFrame(component.InsertingState, 0f),
                                new AnimationTrackSpriteFlick.KeyFrame("icon", 2.4f),
                            },
                        },
                    },
                },
                "faxecute");
        }
    }
}