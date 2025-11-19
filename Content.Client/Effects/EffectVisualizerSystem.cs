// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Client.Animations;
using Robust.Client.GameObjects;

namespace Content.Client.Effects;

public sealed class EffectVisualizerSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<EffectVisualsComponent, AnimationCompletedEvent>(OnEffectAnimComplete);
    }

    private void OnEffectAnimComplete(EntityUid uid, EffectVisualsComponent component, AnimationCompletedEvent args)
    {
        QueueDel(uid);
    }
}
