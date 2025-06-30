// SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Client.Lock.Visualizers;

[RegisterComponent]
[Access(typeof(LockVisualizerSystem))]
public sealed partial class LockVisualsComponent : Component
{
    /// <summary>
    /// The RSI state used for the lock indicator while the entity is locked.
    /// </summary>
    [DataField("stateLocked")]
    [ViewVariables(VVAccess.ReadWrite)]
    public string? StateLocked = "locked";

    /// <summary>
    /// The RSI state used for the lock indicator entity is unlocked.
    /// </summary>
    [DataField("stateUnlocked")]
    [ViewVariables(VVAccess.ReadWrite)]
    public string? StateUnlocked = "unlocked";
}