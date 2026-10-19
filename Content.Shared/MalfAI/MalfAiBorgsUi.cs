// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.MalfAI;

[Serializable, NetSerializable]
public enum MalfAiBorgsUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class MalfAiBorgsUiState : BoundUserInterfaceState
{
    public List<MalfAiBorgListEntry> Borgs;

    public MalfAiBorgsUiState(List<MalfAiBorgListEntry> borgs)
    {
        Borgs = borgs;
    }
}

[DataRecord, Serializable, NetSerializable]
public partial record struct MalfAiBorgListEntry
{
    [DataField]
    public string UniqueId;
    [DataField]
    public string Name;
    [DataField]
    public SpriteSpecifier? ChassisSprite;
    /// <summary>
    /// Health fraction from 0.0 (destroyed) to 1.0 (full health).
    /// </summary>
    [DataField]
    public float HealthFraction;

    /// <summary>
    /// Whether this borg is syncing its laws to the master lawset.
    /// </summary>
    [DataField]
    public bool Synced;

    /// <summary>
    /// Whether this borg is currently in critical state (at or above critical damage threshold).
    /// </summary>
    [DataField]
    public bool IsCritical;

    public MalfAiBorgListEntry(string uniqueId, string name, SpriteSpecifier? chassisSprite, float healthFraction, bool synced = false, bool isCritical = false)
    {
        UniqueId = uniqueId;
        Name = name;
        ChassisSprite = chassisSprite;
        HealthFraction = healthFraction;
        Synced = synced;
        IsCritical = isCritical;
    }
}

/// <summary>
/// Client->Server request to open the selected borg's Laws UI for editing.
/// </summary>
[Serializable, NetSerializable]
public sealed class MalfAiBorgsUpdateLawsMessage : BoundUserInterfaceMessage
{
    public readonly string UniqueId;

    public MalfAiBorgsUpdateLawsMessage(string uniqueId)
    {
        UniqueId = uniqueId;
    }
}

/// <summary>
/// Client->Server request to set/unset sync-to-master for a borg.
/// </summary>
[Serializable, NetSerializable]
public sealed class MalfAiBorgsSetSyncMessage : BoundUserInterfaceMessage
{
    public readonly string UniqueId;
    public readonly bool Enabled;

    public MalfAiBorgsSetSyncMessage(string uniqueId, bool enabled)
    {
        UniqueId = uniqueId;
        Enabled = enabled;
    }
}

/// <summary>
/// Client->Server request to open the Master Lawset editor from the same BUI.
/// </summary>
[Serializable, NetSerializable]
public sealed class MalfAiOpenMasterLawsetMessage : BoundUserInterfaceMessage
{
}

/// <summary>
/// Client->Server request to warp the AI eye to the selected borg.
/// </summary>
[Serializable, NetSerializable]
public sealed class MalfAiBorgsJumpToBorgMessage : BoundUserInterfaceMessage
{
    public readonly string UniqueId;

    public MalfAiBorgsJumpToBorgMessage(string uniqueId)
    {
        UniqueId = uniqueId;
    }
}
