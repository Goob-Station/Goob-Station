// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Client.Mech;

/// <summary>
/// This is used for visualizing mech constructions
/// </summary>
[RegisterComponent]
public sealed partial class MechAssemblyVisualsComponent : Component
{
    /// <summary>
    /// The prefix that is followed by the number which
    /// denotes the current state to use.
    /// </summary>
    [DataField("statePrefix", required: true)]
    public string StatePrefix = string.Empty;
}