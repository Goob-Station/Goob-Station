// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._White.Blocking;
using Content.Shared.Item.ItemToggle.Components;
using Robust.Shared.Audio.Systems;

namespace Content.Client._White.Blocking;

public sealed class RechargeableBlockingSystem : SharedRechargeableBlockingSystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<ForceTurnOffToggleActiveSound>(OnForceTurnOffActiveSound);
    }

    private void OnForceTurnOffActiveSound(ForceTurnOffToggleActiveSound ev)
    {
        if (!TryGetEntity(ev.ToggleItem, out var uid) || !TryComp(uid, out ItemToggleActiveSoundComponent? activeSound))
            return;

        activeSound.PlayingStream = _audio.Stop(activeSound.PlayingStream);
    }
}
