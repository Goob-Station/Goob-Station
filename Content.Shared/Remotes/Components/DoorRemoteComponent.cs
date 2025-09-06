// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <plykiya@protonmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Remotes.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DoorRemoteComponent : Component
{
    [AutoNetworkedField]
    [DataField]
    public OperatingMode Mode = OperatingMode.OpenClose;

    /// <summary>
    /// Does the remote allow the user to manipulate doors that they have access to, even if the remote itself does not?
    /// </summary>
    [AutoNetworkedField]
    [DataField]
    public bool IncludeUserAccess = false;
}

public enum OperatingMode : byte
{
    OpenClose,
    ToggleBolts,
    ToggleEmergencyAccess,
    placeholderForUiUpdates
}