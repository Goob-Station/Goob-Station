// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Lidgren.Network;
using static Content.Goobstation.Server.Voice.VoiceChatServerManager;

namespace Content.Goobstation.Server.Voice;

/// <summary>
/// Interface for the server-side voice chat manager.
/// </summary>
public interface IVoiceChatServerManager : IDisposable
{
    void Update();
    Dictionary<NetConnection, VoiceClientData> Clients { get; }
}
