// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._RMC14.LinkAccount;

[Serializable, NetSerializable]
public sealed class SharedRMCShowLobbyMessageEvent(string text) : EntityEventArgs
{
    public readonly string Text = text;
}