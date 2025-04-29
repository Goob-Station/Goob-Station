using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.VoiceChat;
using Robust.Shared.Configuration;
using Robust.Shared.Network;

namespace Content.Goobstation.Client.Voice;

public sealed class VoiceChatClientManager : IVoiceChatManager, IDisposable
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly INetManager _netManager = default!;

    private ISawmill _sawmill = default!;
    private readonly Dictionary<EntityUid, VoiceStreamManager> _activeStreams = new();

    private float _volumeCVarValue = 0.5f;

    public void Initalize()
    {
        IoCManager.InjectDependencies(this);
        _sawmill = Logger.GetSawmill("voice.client");

        _cfg.OnValueChanged(GoobCVars.VoiceChatVolume, OnVolumeChanged, true);

        _netManager.RegisterNetMessage<MsgVoiceChat>(OnVoiceMessageReceived);

        _sawmill.Info("VoiceChatClientManager initialized");
    }

    private void OnVolumeChanged(float volume)
    {

        _volumeCVarValue = volume;
        _sawmill.Debug($"Voice chat base volume CVar changed to {volume}. This affects newly created streams.");
        foreach (var stream in _activeStreams.Values)
            stream.SetVolume(volume);

    }

    private void OnVoiceMessageReceived(MsgVoiceChat message)
    {
        if (message.PcmData == null || message.SourceEntity == null)
        {
            _sawmill.Warning("Received invalid voice chat message (null data or source)");
            return;
        }

        var sourceUid = _entityManager.GetEntity(message.SourceEntity.Value);
        if (!sourceUid.IsValid() || !_entityManager.EntityExists(sourceUid))
        {
            _sawmill.Debug($"Received voice chat message for invalid or non-existent entity: {message.SourceEntity}");
            return;
        }

        AddPacket(sourceUid, message.PcmData);
    }

    /// <inheritdoc/>
    public void AddPacket(EntityUid sourceEntity, byte[] pcmData)
    {
        if (!TryGetStreamManager(sourceEntity, out var streamManager))
        {
            _sawmill.Debug($"Creating new voice stream for entity {sourceEntity}");

            streamManager = new VoiceStreamManager(sourceEntity);
            streamManager.SetVolume(_volumeCVarValue);
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
            stream.Dispose();

        _activeStreams.Clear();

        _sawmill.Info("VoiceChatClientManager disposed");
    }

    /// <inheritdoc/>
    public void Update()
    {
        List<EntityUid>? toRemove = null;

        foreach (var uid in _activeStreams.Keys)
        {

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
                if (_activeStreams.Remove(uid, out var stream))
                {
                    _sawmill.Debug($"Removing voice stream for deleted entity {uid}");
                    stream.Dispose();
                }
            }
        }
    }
}
