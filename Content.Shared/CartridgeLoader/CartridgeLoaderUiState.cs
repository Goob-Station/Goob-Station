// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared.CartridgeLoader;

[Virtual]
[Serializable, NetSerializable]
public class CartridgeLoaderUiState : BoundUserInterfaceState
{
    public NetEntity? ActiveUI;
    public List<NetEntity> Programs;

    public CartridgeLoaderUiState(List<NetEntity> programs, NetEntity? activeUI)
    {
        Programs = programs;
        ActiveUI = activeUI;
    }
}