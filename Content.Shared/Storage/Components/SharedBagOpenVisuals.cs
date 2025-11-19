// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.Serialization;

namespace Content.Shared.Storage.Components
{
    [Serializable, NetSerializable]
    public enum SharedBagOpenVisuals : byte
    {
        BagState,
    }

    [Serializable, NetSerializable]
    public enum SharedBagState : byte
    {
        Open,
        Closed,
    }
}
