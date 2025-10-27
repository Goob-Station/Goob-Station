// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.Explosion.Components;

/// <summary>
/// Triggers when the parent entity speaks.
/// </summary>
[RegisterComponent]
public sealed partial class TriggerOnSpeakComponent : Component
{
    /// <summary>
    ///     The range at which it listens for keywords.
    /// </summary>
    [DataField]
    public int ListenRange { get; private set; } = 4;
}

