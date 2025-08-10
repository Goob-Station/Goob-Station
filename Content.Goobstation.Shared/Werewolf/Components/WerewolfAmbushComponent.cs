// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.Werewolf.Components;

[RegisterComponent]
public sealed partial class WerewolfAmbushComponent : Component
{
    [DataField]
    public TimeSpan StartTime = TimeSpan.Zero;

    /// <summary>
    ///  The amount of time it takes to leap to an entity
    /// </summary>
    [DataField]
    public TimeSpan LeapDuration = TimeSpan.FromSeconds(0.2);

    /// <summary>
    ///  The target to leap to
    /// </summary>
    [ViewVariables]
    public EntityUid? Target;

    /// <summary>
    ///  Whether the ability got activated, or not.
    /// </summary>
    [ViewVariables]
    public bool Active;

    /// <summary>
    ///  The duration that applies to anyone that got leaped
    /// </summary>
    [DataField]
    public TimeSpan StunDuration = TimeSpan.FromSeconds(5);

    /// <summary>
    ///  The range of the stun
    /// </summary>
    [DataField]
    public float Range = 1f;
}
