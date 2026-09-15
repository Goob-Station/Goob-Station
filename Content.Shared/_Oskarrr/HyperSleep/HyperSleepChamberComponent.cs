// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared._Oskarrr.HyperSleep;

/// <summary>
/// Marks a structure as a hypersleep chamber (enter/exit container only).
/// </summary>
[RegisterComponent]
public sealed partial class HyperSleepChamberComponent : Component
{
    [DataField]
    public string ContainerId = "storage";
}

[Serializable, NetSerializable]
public enum HyperSleepChamberVisuals : byte
{
    Occupied
}
