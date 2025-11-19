// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

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
