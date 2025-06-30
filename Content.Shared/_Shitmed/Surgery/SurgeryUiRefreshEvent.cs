// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._Shitmed.Medical.Surgery;

[Serializable, NetSerializable]
public sealed class SurgeryUiRefreshEvent : EntityEventArgs
{
    public NetEntity Uid { get; }

    public SurgeryUiRefreshEvent(NetEntity uid)
    {
        Uid = uid;
    }
}