// SPDX-License-Identifier: MIT

using Content.Shared.Shuttles.Components;

namespace Content.Client.Shuttles;

[RegisterComponent]
[AutoGenerateComponentState] // Frontier
public sealed partial class ShuttleConsoleComponent : SharedShuttleConsoleComponent
{
    /// <summary>
    /// Frontier edit
    /// Custom display names for network port buttons.
    /// Key is the port ID, value is the display name.
    /// </summary>
    [DataField("portLabels"), AutoNetworkedField]
    public new Dictionary<string, string> PortNames = new();
}
