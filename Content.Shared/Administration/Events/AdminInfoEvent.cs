// SPDX-FileCopyrightText: 2024 Conchelle <mary@thughunt.ing>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared.Administration.Events;

[Serializable, NetSerializable]
public sealed class AdminInfoEvent(NetUserId userid) : EntityEventArgs
{
    public NetUserId user = userid;
}