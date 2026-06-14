// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.MalfAI.Borgs;

[Serializable, NetSerializable]
public enum MalfAiBorgsUiKey : byte
{
    Key,
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

[Serializable, NetSerializable]
public sealed class MalfAiBorgListEntry
{
    public NetEntity Borg;
    public string Name;
    public float HealthFraction;
    public bool IsSynced;
    public List<string> Laws;
    public bool IsOwned;
    public bool OwnedByMe;
    public bool IsBeingClaimed;

    public MalfAiBorgListEntry(NetEntity borg, string name, float healthFraction, bool isSynced, List<string> laws, bool isOwned, bool ownedByMe, bool isBeingClaimed = false)
    {
        Borg = borg;
        Name = name;
        HealthFraction = healthFraction;
        IsSynced = isSynced;
        Laws = laws;
        IsOwned = isOwned;
        OwnedByMe = ownedByMe;
        IsBeingClaimed = isBeingClaimed;
    }
}

[Serializable, NetSerializable]
public sealed class MalfAiBorgsSetSyncMessage : BoundUserInterfaceMessage
{
    public NetEntity Borg;
    public bool Synced;

    public MalfAiBorgsSetSyncMessage(NetEntity borg, bool synced)
    {
        Borg = borg;
        Synced = synced;
    }
}

[Serializable, NetSerializable]
public sealed class MalfAiClaimBorgMessage : BoundUserInterfaceMessage
{
    public NetEntity Borg;

    public MalfAiClaimBorgMessage(NetEntity borg)
    {
        Borg = borg;
    }
}

[Serializable, NetSerializable]
public sealed class MalfAiResyncAllBorgsMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class MalfAiOpenMasterLawsetMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class MalfAiBorgsJumpToBorgMessage : BoundUserInterfaceMessage
{
    public NetEntity Borg;

    public MalfAiBorgsJumpToBorgMessage(NetEntity borg)
    {
        Borg = borg;
    }
}
