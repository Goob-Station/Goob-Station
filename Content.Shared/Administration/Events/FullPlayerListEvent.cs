// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.Serialization;

namespace Content.Shared.Administration.Events
{
    [Serializable, NetSerializable]
    public sealed class FullPlayerListEvent : EntityEventArgs
    {
        public List<PlayerInfo> PlayersInfo = new();
    }
}
