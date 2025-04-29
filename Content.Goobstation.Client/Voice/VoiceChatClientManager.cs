using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.VoiceChat;
using Robust.Client.Audio;
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
    private AudioSystem? _audioSystem = default!;

    private ISawmill _sawmill = default!;
    private readonly Dictionary<EntityUid, VoiceStreamManager> _activeStreams = new();

    private int _sampleRate = 48000;
    private float _volume = 0.5f;

    public void Initalize()
    {
        IoCManager.InjectDependencies(this);
        _sawmill = Logger.GetSawmill("voiceclient");

        _cfg.OnValueChanged(GoobCVars.VoiceChatVolume, OnVolumeChanged, true);

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

        if (!TryGetStreamManager(sourceEntity, out var streamManager))
        {
            _sawmill.Debug($"Creating new voice stream for entity {sourceEntity}");
            streamManager = new VoiceStreamManager(_audioManager, _audioSystem, sourceEntity, _sampleRate);
            streamManager.SetVolume(_volume);
            AddStreamManager(sourceEntity, streamManager);
        }

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
