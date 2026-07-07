// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Store.Components;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Changeling.Components;

[RegisterComponent, NetworkedComponent]

public sealed partial class ChangelingEggComponent : Component
{
    public ChangelingIdentityComponent lingComp;
    public EntityUid lingMind;
    public StoreComponent lingStore;

    /// <summary>
    ///     Countdown before spawning monkey.
    /// </summary>
    public TimeSpan UpdateTimer = TimeSpan.Zero;
    public float UpdateCooldown = 120f;
    public bool active = false;
}
