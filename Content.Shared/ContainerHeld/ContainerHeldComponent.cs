// SPDX-FileCopyrightText: 2023 Bixkitts <72874643+Bixkitts@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.ContainerHeld;

[RegisterComponent, NetworkedComponent]
public sealed partial class ContainerHeldComponent: Component
{
    /// <summary>
    ///     The amount of weight needed to be in the container
    ///     in order for it to toggle it's appearance
    ///     to ToggleVisuals.Toggled = true, and
    ///     SetHeldPrefix() to "full" instead of "empty".
    /// </summary>
    [DataField("threshold")]
    public int Threshold { get; private set; } = 1;
}