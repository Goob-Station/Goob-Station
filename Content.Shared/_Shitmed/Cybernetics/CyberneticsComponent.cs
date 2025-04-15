// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Cybernetics;

/// <summary>
/// Component for cybernetic implants that can be installed in entities
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CyberneticsComponent : Component 
{
    /// <summary>
    ///     Is the cybernetic implant disabled by EMPs, etc?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Disabled = false;
}