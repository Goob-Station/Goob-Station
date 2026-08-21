// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.CriminalRecords.Systems;
using Robust.Shared.Serialization;

namespace Content.Shared.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class WantedListUiState(List<WantedRecord> records) : BoundUserInterfaceState
{
    public List<WantedRecord> Records = records;
}