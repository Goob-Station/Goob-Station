using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.VoiceChat;
using Content.Shared.GameTicking;
using Robust.Client.Audio;
using Robust.Client.Graphics;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.VoiceChat;

public sealed class VoiceChatSystem : EntitySystem
{
    [Dependency] private readonly VoiceChatManager _manager = default!;
    [Dependency] private readonly IAudioManager _audioManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IOverlayManager _overlays = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private static readonly TimeSpan IdleTimeout = TimeSpan.FromSeconds(10);

    private readonly Dictionary<ushort, VoicePlaybackStream> _streams = new();
    private readonly List<ushort> _removeQueue = new();

    private float _range;
    private float _volume;

    public override void Initialize()
    {
        base.Initialize();

        UpdatesOutsidePrediction = true;

        _manager.FrameReceived += OnFrameReceived;

        Subs.CVar(_cfg, GoobCVars.VoiceChatRange, value => _range = value, true);
        Subs.CVar(_cfg, GoobCVars.VoiceChatVolume, value => _volume = Math.Clamp(value, 0f, 2f), true);
        Subs.CVar(_cfg, GoobCVars.VoiceChatEnabled, enabled =>
        {
            if (!enabled)
                ClearStreams();
        });

        SubscribeNetworkEvent<RoundRestartCleanupEvent>(_ => ClearStreams());

        _overlays.AddOverlay(new VoiceSpeakingOverlay(EntityManager, _timing));
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _manager.FrameReceived -= OnFrameReceived;
        _overlays.RemoveOverlay<VoiceSpeakingOverlay>();
        ClearStreams();
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        if (_streams.Count == 0)
            return;

        var now = _timing.RealTime;
        var listener = _audio.GetListenerCoordinates();

        foreach (var (id, stream) in _streams)
        {
            stream.Update(now, GetParams(stream.Source, listener));

            if (stream.IsIdle && now - stream.LastActivity > IdleTimeout)
                _removeQueue.Add(id);
        }

        foreach (var id in _removeQueue)
        {
            if (_streams.Remove(id, out var stream))
                stream.Dispose();
        }

        _removeQueue.Clear();
    }

    private void OnFrameReceived(MsgVoiceFrame message)
    {
        var pcm = new short[VoiceCodec.FrameSamples];
        if (!VoiceCodec.Decode(message.Payload, pcm))
            return;

        if (!_streams.TryGetValue(message.Speaker, out var stream))
        {
            stream = new VoicePlaybackStream(_audioManager);
            _streams[message.Speaker] = stream;
        }

        stream.Source = message.Source;
        stream.AddFrame(message.Sequence, message.Flags, pcm, _timing.RealTime);
    }

    private VoicePlaybackParams GetParams(NetEntity source, MapCoordinates listener)
    {
        var referenceDistance = _audio.GetAudioDistance(1f);
        var maxDistance = _audio.GetAudioDistance(_range);
        var gain = MathF.Min(_volume, 1f);
        var boost = MathF.Max(_volume, 1f);

        if (!TryGetEntity(source, out var uid) || TerminatingOrDeleted(uid.Value))
            return new VoicePlaybackParams(listener.Position, 0f, 0f, referenceDistance, maxDistance, boost);

        var coordinates = _transform.GetMapCoordinates(uid.Value);
        if (coordinates.MapId != listener.MapId)
            return new VoicePlaybackParams(listener.Position, 0f, 0f, referenceDistance, maxDistance, boost);

        var delta = coordinates.Position - listener.Position;
        var distance = delta.Length();
        if (_audio.GetAudioDistance(distance) > maxDistance)
            return new VoicePlaybackParams(coordinates.Position, 0f, 0f, referenceDistance, maxDistance, boost);

        var occlusion = distance > 0.1f ? _audio.GetOcclusion(listener, delta, distance, uid.Value) : 0f;
        return new VoicePlaybackParams(coordinates.Position, gain, occlusion, referenceDistance, maxDistance, boost);
    }

    private void ClearStreams()
    {
        foreach (var stream in _streams.Values)
        {
            stream.Dispose();
        }

        _streams.Clear();
    }
}
