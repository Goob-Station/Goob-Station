// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Pinpointer;

[RegisterComponent]
public sealed partial class StationMapComponent : Component
{
    /// <summary>
    /// Whether or not to show the user's location on the map.
    /// </summary>
    [DataField]
    public bool ShowLocation = true;
}