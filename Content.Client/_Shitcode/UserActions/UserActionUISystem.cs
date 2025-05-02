// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Player;

namespace Content.Client._Shitcode.UserActions;

public sealed class UserActionUISystem : EntitySystem
{
    public event Action? PlayerAttachedEvent;
    public event Action? PlayerDetachedEvent;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LocalPlayerAttachedEvent>(LocalPlayerAttached);
        SubscribeLocalEvent<LocalPlayerDetachedEvent>(LocalPlayerDetached);
    }

    private void LocalPlayerAttached(LocalPlayerAttachedEvent ev)
        => PlayerAttachedEvent?.Invoke();

    private void LocalPlayerDetached(LocalPlayerDetachedEvent ev)
        => PlayerDetachedEvent?.Invoke();
}
