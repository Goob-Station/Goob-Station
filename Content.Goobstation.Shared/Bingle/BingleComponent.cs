// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Bingle;

[RegisterComponent, NetworkedComponent]
public sealed partial class BingleComponent : Component
{
    [DataField]
    public bool Upgraded = false;
    [DataField]
    public bool Prime = false;
    [DataField]
    public EntityUid? MyPit;
    [DataField]
    public EntProtoId? UpgradedID;

    /// <summary>
    /// Will receive SeethingComponent if valid whitelist passing entity is within this range.
    /// </summary>
    [DataField]
    public float RageRange = 12f;

    [DataField]
    public TimeSpan RageCheckInterval = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Component added when nearby valid whitelist passing entity.
    /// </summary>
    [DataField]
    public ComponentRegistry CompToAdd = new();

    /// <summary>
    /// If false don't link to pit and don't spawn a pit.
    /// </summary>
    [DataField]
    public bool LinkToPit = true;
}

[Serializable, NetSerializable]
public enum BingleVisual : byte
{
    Upgraded,
    Combat
}
