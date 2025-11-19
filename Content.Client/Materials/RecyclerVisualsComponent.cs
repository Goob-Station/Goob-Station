// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Client.Materials;

[RegisterComponent]
public sealed partial class RecyclerVisualsComponent : Component
{
    /// <summary>
    /// Key appended to state string if bloody.
    /// </summary>
    [DataField]
    public string BloodyKey = "bld";

    /// <summary>
    /// Base key for the visual state.
    /// </summary>
    [DataField]
    public string BaseKey = "grinder-o";
}
