// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
//
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
