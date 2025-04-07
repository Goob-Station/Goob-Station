// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
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