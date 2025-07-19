// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.VoiceChat;
using Robust.Client.Audio;
using Robust.Client.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Network;

namespace Content.Goobstation.Client.Voice;

/// <summary>
/// Client-side manager for voice chat functionality.
/// Handles network messages and manages voice streams.
/// </summary>
public sealed class VoiceChatClientManager : IVoiceChatManager
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IAudioManager _audioManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    private AudioSystem? _audioSystem = default!;

    private ISawmill _sawmill = default!;
    private readonly Dictionary<EntityUid, VoiceStreamManager> _activeStreams = new();

    private int _sampleRate = 48000;
    private float _volume = 0.5f;
    private bool _hearSelf = false;

    public void Initalize()
    {
        IoCManager.InjectDependencies(this);
        _sawmill = Logger.GetSawmill("voiceclient");

        _volume = _cfg.GetCVar(GoobCVars.VoiceChatVolume);
        _hearSelf = _cfg.GetCVar(GoobCVars.VoiceChatHearSelf);
        _sawmill.Info($"VoiceChatClientManager initialized with volume: {_volume}, hear_self: {_hearSelf}");
        _cfg.OnValueChanged(GoobCVars.VoiceChatVolume, OnVolumeChanged, true);
        _cfg.OnValueChanged(GoobCVars.VoiceChatHearSelf, OnHearSelfChanged, true);

        _netManager.RegisterNetMessage<MsgVoiceChat>(OnVoiceMessageReceived);

        _sawmill.Info("VoiceChatClientManager initialized");
    }

    /// <summary>
    /// Handle volume changes from CVars.
    /// </summary>
    private void OnVolumeChanged(float volume)
    {
        _volume = volume;

        foreach (var stream in _activeStreams.Values)
        {
            stream.SetVolume(_volume);
        }

        _sawmill.Debug($"Voice chat volume changed to {volume}");
    }

    /// <summary>
    /// Handle hear_self changes from CVars.
    /// </summary>
    private void OnHearSelfChanged(bool hearSelf)
    {
        _hearSelf = hearSelf;
        _sawmill.Debug($"Voice chat hear_self changed to {hearSelf}");
    }

    /// <summary>
    /// Handle incoming voice chat network messages.
    /// </summary>
    private void OnVoiceMessageReceived(MsgVoiceChat message)
    {
        if (message.PcmData == null || message.SourceEntity == null)
        {
            _sawmill.Warning("Received invalid voice chat message (null data or source)");
            return;
        }

        var sourceUid = _entityManager.GetEntity(message.SourceEntity.Value);
        if (!sourceUid.IsValid())
        {
            _sawmill.Warning($"Received voice chat message for invalid entity: {message.SourceEntity}");
            return;
        }

        AddPacket(sourceUid, message.PcmData);
    }

    /// <inheritdoc/>
    public void AddPacket(EntityUid sourceEntity, byte[] pcmData)
    {
        _audioSystem ??= _entityManager.System<AudioSystem>();

        var localPlayer = _playerManager.LocalEntity;
        if (localPlayer == sourceEntity && !_hearSelf)
        {
            _sawmill.Debug($"[VOICE DEBUG] Filtering out audio from own entity {sourceEntity} (hear_self disabled)");
            return;
        }

        if (!TryGetStreamManager(sourceEntity, out var streamManager))
        {
            _sawmill.Info($"[VOICE DEBUG] Creating new voice stream for entity {sourceEntity}");
            streamManager = new VoiceStreamManager(_audioManager, _audioSystem, sourceEntity, _sampleRate);
            streamManager.SetVolume(_volume);
            AddStreamManager(sourceEntity, streamManager);
        }
        else
        {
            _sawmill.Debug($"[VOICE DEBUG] Using existing voice stream for entity {sourceEntity}");
        }

        _sawmill.Debug($"[VOICE DEBUG] Adding packet to stream for entity {sourceEntity} (stream count: {_activeStreams.Count})");
        streamManager.AddPacket(pcmData);
    }

    /// <inheritdoc/>
    public bool TryGetStreamManager(EntityUid sourceEntity, out VoiceStreamManager streamManager)
    {
        if (_activeStreams.TryGetValue(sourceEntity, out var manager))
        {
            streamManager = manager;
            return true;
        }

        streamManager = null!;
        return false;
    }

    /// <inheritdoc/>
    public void AddStreamManager(EntityUid sourceEntity, VoiceStreamManager streamManager)
    {
        _activeStreams[sourceEntity] = streamManager;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _cfg.UnsubValueChanged(GoobCVars.VoiceChatVolume, OnVolumeChanged);
        _cfg.UnsubValueChanged(GoobCVars.VoiceChatHearSelf, OnHearSelfChanged);

        foreach (var stream in _activeStreams.Values)
        {
            stream.Dispose();
        }
        _activeStreams.Clear();

        _sawmill.Info("VoiceChatClientManager disposed");
    }

    /// <inheritdoc/>
    public void Update()
    {
        List<EntityUid>? toRemove = null;

        foreach (var (uid, stream) in _activeStreams)
        {
            stream.Update();

            if (!_entityManager.EntityExists(uid))
            {
                toRemove ??= new List<EntityUid>();
                toRemove.Add(uid);
            }
        }

        if (toRemove != null)
        {
            foreach (var uid in toRemove)
            {
                if (_activeStreams.TryGetValue(uid, out var stream))
                {
                    _sawmill.Debug($"Removing voice stream for deleted entity {uid}");
                    stream.Dispose();
                    _activeStreams.Remove(uid);
                }
            }
        }
    }
}
