// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Wizard.FadingTimedDespawn;
using Robust.Client.Animations;
using Robust.Client.GameObjects;

namespace Content.Client._Shitcode.Wizard.Systems;

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