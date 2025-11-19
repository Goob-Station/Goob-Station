// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.StationRecords;

[RegisterComponent, NetworkedComponent]
public sealed partial class StationRecordKeyStorageComponent : Component
{
    /// <summary>
    ///     The key stored in this component.
    /// </summary>
    [ViewVariables]
    public StationRecordKey? Key;
}

[Serializable, NetSerializable]
public sealed class StationRecordKeyStorageComponentState : ComponentState
{
    public (NetEntity, uint)? Key;

    public StationRecordKeyStorageComponentState((NetEntity, uint)? key)
    {
        Key = key;
    }
}
