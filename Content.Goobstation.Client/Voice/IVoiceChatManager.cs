// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Client.Voice;

/// <summary>
/// Interface for the voice chat manager.
/// </summary>
public interface IVoiceChatManager : IDisposable
{
    /// <summary>
    /// Adds a packet of PCM audio data to the playback queue for a specific entity.
    /// </summary>
    /// <param name="sourceEntity">The entity that is the source of the voice.</param>
    /// <param name="pcmData">The PCM audio data to add.</param>
    void AddPacket(EntityUid sourceEntity, byte[] pcmData);

    /// <summary>
    /// Tries to get the stream manager for a specific entity.
    /// </summary>
    /// <param name="sourceEntity">The entity to get the stream manager for.</param>
    /// <param name="streamManager">The stream manager, if found.</param>
    /// <returns>True if the stream manager was found, false otherwise.</returns>
    bool TryGetStreamManager(EntityUid sourceEntity, out VoiceStreamManager streamManager);

    /// <summary>
    /// Adds a stream manager for a specific entity.
    /// </summary>
    /// <param name="sourceEntity">The entity to add the stream manager for.</param>
    /// <param name="streamManager">The stream manager to add.</param>
    void AddStreamManager(EntityUid sourceEntity, VoiceStreamManager streamManager);

    void Initalize();
    void Update();
}
