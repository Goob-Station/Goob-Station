// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.BUIStates;

[Serializable, NetSerializable]
public sealed class EmergencyConsoleBoundUserInterfaceState : BoundUserInterfaceState
{
    /// <summary>
    /// null if we're not early launching.
    /// </summary>
    public TimeSpan? EarlyLaunchTime;
    public List<string> Authorizations = new();
    public int AuthorizationsRequired;

    public TimeSpan? TimeToLaunch;
}