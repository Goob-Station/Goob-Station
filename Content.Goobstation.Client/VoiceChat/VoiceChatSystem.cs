using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.VoiceChat;
using Content.Shared.GameTicking;
using Content.Shared.Radio;
using Content.Shared.Verbs;
using Robust.Client.Audio;
using Robust.Client.Graphics;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.VoiceChat;

public readonly record struct VoiceSpeakerInfo(string Name, ProtoId<RadioChannelPrototype>? Channel);

public sealed class VoiceSelfState
{
    public ushort Speaker;
    public VoiceSelfFlags Flags;
    public VoiceLevels Levels;
    public VoiceLevels Target;
    public float Activity;
    public TimeSpan LastReceived;
}

public sealed class VoiceChatSystem : EntitySystem
{
    [Dependency] private readonly VoiceChatManager _manager = default!;
    [Dependency] private readonly IAudioManager _audioManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IOverlayManager _overlays = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private static readonly TimeSpan IdleTimeout = TimeSpan.FromSeconds(10);
    private static readonly float[] VolumeLevels = { 0.25f, 0.5f, 1f, 1.5f, 2f };
    private static readonly SpriteSpecifier VoiceIcon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/settings.svg.192dpi.png"));
    private static readonly SpriteSpecifier MuteIcon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/close.svg.192dpi.png"));

    private static readonly Color TelephoneColor = Color.FromHex("#9FED58");
    private static readonly Color HolopadColor = Color.FromHex("#5CE1E6");
    private static readonly Color CameraColor = Color.FromHex("#7FB2FF");
    private static readonly Color BroadcastColor = Color.FromHex("#FFC844");
    private static readonly Color RadioColor = Color.FromHex("#2CDB2C");
    private static readonly Color BlockedColor = Color.FromHex("#E04545");
    private static readonly TimeSpan SelfTimeout = TimeSpan.FromMilliseconds(150);

    private const float OcclusionVisibility = 0.35f;
    private const float SelfActivityRelease = 0.3f;

    private readonly Dictionary<ushort, VoicePlaybackStream> _streams = new();
    private readonly List<ushort> _removeQueue = new();
    private readonly Dictionary<EntityUid, ushort> _knownSpeakers = new();
    private readonly Dictionary<ushort, VoiceSpeakerInfo> _speakerInfo = new();
    private readonly HashSet<ushort> _muted = new();
    private readonly Dictionary<ushort, float> _speakerVolume = new();

    private float _range;
    private float _volume;
    private float _radioVolume;

    public IReadOnlyDictionary<ushort, VoicePlaybackStream> Streams => _streams;

    public VoiceSelfState Self { get; } = new();

    public override void Initialize()
    {
        base.Initialize();

        UpdatesOutsidePrediction = true;

        _manager.FrameReceived += OnFrameReceived;
        _manager.SpeakerInfoReceived += OnSpeakerInfoReceived;
        _manager.SelfReceived += OnSelfReceived;

        Subs.CVar(_cfg, GoobCVars.VoiceChatRange, value => _range = value, true);
        Subs.CVar(_cfg, GoobCVars.VoiceChatVolume, value => _volume = Math.Clamp(value, 0f, 2f), true);
        Subs.CVar(_cfg, GoobCVars.VoiceChatRadioVolume, value => _radioVolume = Math.Clamp(value, 0f, 2f), true);
        Subs.CVar(_cfg, GoobCVars.VoiceChatEnabled, enabled =>
        {
            if (!enabled)
                ClearStreams();
        });

        SubscribeNetworkEvent<RoundRestartCleanupEvent>(_ => OnRoundRestart());
        SubscribeLocalEvent<GetVerbsEvent<Verb>>(OnGetVerbs);

        _overlays.AddOverlay(new VoiceSpeakingOverlay(EntityManager, this));
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _manager.FrameReceived -= OnFrameReceived;
        _manager.SpeakerInfoReceived -= OnSpeakerInfoReceived;
        _manager.SelfReceived -= OnSelfReceived;
        _overlays.RemoveOverlay<VoiceSpeakingOverlay>();
        ClearStreams();
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var now = _timing.RealTime;
        UpdateSelf(now, frameTime);

        if (_streams.Count == 0)
            return;

        var listener = _audio.GetListenerCoordinates();

        foreach (var (id, stream) in _streams)
        {
            stream.Update(now, GetParams(stream, listener));
            stream.UpdateLevels(frameTime);

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

    public bool IsSelf(ushort speaker)
    {
        return Self.Speaker != 0 && Self.Speaker == speaker;
    }

    public Color GetSelfColor()
    {
        if ((Self.Flags & VoiceSelfFlags.Blocked) != 0)
            return BlockedColor;

        if ((Self.Flags & VoiceSelfFlags.Broadcast) != 0)
            return BroadcastColor;

        if ((Self.Flags & VoiceSelfFlags.Radio) != 0)
            return TryGetRadioChannel(Self.Speaker, out var channel) ? channel.Color : RadioColor;

        return Color.White;
    }

    public string? GetSelfLabel()
    {
        if ((Self.Flags & VoiceSelfFlags.Blocked) != 0)
            return Loc.GetString("voice-self-blocked");

        if ((Self.Flags & VoiceSelfFlags.Broadcast) != 0)
            return Loc.GetString("voice-route-broadcast");

        if ((Self.Flags & VoiceSelfFlags.Radio) != 0)
        {
            return TryGetRadioChannel(Self.Speaker, out var channel)
                ? channel.LocalizedName
                : Loc.GetString("voice-route-radio");
        }

        return null;
    }

    public bool TryGetSpeakerInfo(ushort speaker, out VoiceSpeakerInfo info)
    {
        return _speakerInfo.TryGetValue(speaker, out info);
    }

    public Color GetRouteColor(VoicePlaybackStream stream)
    {
        return stream.Route switch
        {
            VoiceRoute.Radio or VoiceRoute.RadioSpeaker => TryGetRadioChannel(stream.Speaker, out var channel) ? channel.Color : RadioColor,
            VoiceRoute.Telephone => TelephoneColor,
            VoiceRoute.Holopad => HolopadColor,
            VoiceRoute.Camera => CameraColor,
            VoiceRoute.Broadcast => BroadcastColor,
            _ => Color.White,
        };
    }

    public string? GetRouteLabel(VoicePlaybackStream stream)
    {
        return stream.Route switch
        {
            VoiceRoute.Radio or VoiceRoute.RadioSpeaker => TryGetRadioChannel(stream.Speaker, out var channel)
                ? channel.LocalizedName
                : Loc.GetString("voice-route-radio"),
            VoiceRoute.Telephone => Loc.GetString("voice-route-telephone"),
            VoiceRoute.Holopad => Loc.GetString("voice-route-holopad"),
            VoiceRoute.Camera => Loc.GetString("voice-route-camera"),
            VoiceRoute.Broadcast => Loc.GetString("voice-route-broadcast"),
            _ => null,
        };
    }

    private bool TryGetRadioChannel(ushort speaker, [NotNullWhen(true)] out RadioChannelPrototype? channel)
    {
        channel = null;
        return _speakerInfo.TryGetValue(speaker, out var info) &&
               info.Channel is { } id &&
               _prototype.TryIndex(id, out channel);
    }

    private void OnRoundRestart()
    {
        ClearStreams();
        _knownSpeakers.Clear();
    }

    private void OnGetVerbs(GetVerbsEvent<Verb> args)
    {
        if (args.User == args.Target ||
            !_cfg.GetCVar(GoobCVars.VoiceChatEnabled) ||
            !_knownSpeakers.TryGetValue(args.Target, out var speaker))
        {
            return;
        }

        var category = new VerbCategory("voice-verb-category", "/Textures/Interface/VerbIcons/settings.svg.192dpi.png");
        var muted = _muted.Contains(speaker);

        args.Verbs.Add(new Verb
        {
            Text = Loc.GetString(muted ? "voice-verb-unmute" : "voice-verb-mute"),
            Icon = muted ? VoiceIcon : MuteIcon,
            Category = category,
            Priority = 10,
            ClientExclusive = true,
            Act = () => SetMuted(speaker, !muted),
        });

        var current = _speakerVolume.GetValueOrDefault(speaker, 1f);
        for (var i = 0; i < VolumeLevels.Length; i++)
        {
            var level = VolumeLevels[i];
            args.Verbs.Add(new Verb
            {
                Text = Loc.GetString("voice-verb-volume", ("percent", (int) MathF.Round(level * 100f))),
                Category = category,
                Priority = -i,
                Disabled = MathHelper.CloseTo(level, current),
                ClientExclusive = true,
                Act = () => SetSpeakerVolume(speaker, level),
            });
        }
    }

    private void SetMuted(ushort speaker, bool muted)
    {
        if (!muted)
        {
            _muted.Remove(speaker);
            return;
        }

        _muted.Add(speaker);
        if (_streams.Remove(speaker, out var stream))
            stream.Dispose();
    }

    private void SetSpeakerVolume(ushort speaker, float volume)
    {
        if (MathHelper.CloseTo(volume, 1f))
            _speakerVolume.Remove(speaker);
        else
            _speakerVolume[speaker] = volume;
    }

    private void OnSelfReceived(MsgVoiceSelf message)
    {
        Self.Speaker = message.Speaker;
        Self.Flags = message.Flags;
        Self.Target = message.Levels;
        Self.LastReceived = _timing.RealTime;
    }

    private void UpdateSelf(TimeSpan now, float frameTime)
    {
        var active = Self.LastReceived != TimeSpan.Zero && now - Self.LastReceived < SelfTimeout;
        if (!active)
            Self.Target = default;

        VoiceLevelSmoothing.Apply(ref Self.Levels, Self.Target, frameTime);
        Self.Activity = active
            ? 1f
            : MathF.Max(0f, Self.Activity - frameTime / SelfActivityRelease);
    }

    private void OnSpeakerInfoReceived(MsgVoiceSpeakerInfo message)
    {
        ProtoId<RadioChannelPrototype>? channel = null;
        if (!string.IsNullOrEmpty(message.Channel))
            channel = message.Channel;

        _speakerInfo[message.Speaker] = new VoiceSpeakerInfo(message.Name, channel);
    }

    private void OnFrameReceived(MsgVoiceFrame message)
    {
        if (_muted.Contains(message.Speaker))
            return;

        var pcm = new short[VoiceCodec.FrameSamples];
        if (!VoiceCodec.Decode(message.Payload, pcm))
            return;

        if (!_streams.TryGetValue(message.Speaker, out var stream))
        {
            stream = new VoicePlaybackStream(_audioManager);
            _streams[message.Speaker] = stream;
        }

        if (message.Route is VoiceRoute.Direct or VoiceRoute.Camera && TryGetEntity(message.Source, out var source))
            _knownSpeakers[source.Value] = message.Speaker;

        stream.Speaker = message.Speaker;
        stream.Source = message.Source;
        stream.Route = message.Route;
        stream.Global = message.Global;
        stream.Range = message.Range > 0f ? message.Range : _range;
        stream.AddFrame(message.Sequence, message.Flags, pcm, _timing.RealTime);
    }

    private VoicePlaybackParams GetParams(VoicePlaybackStream stream, MapCoordinates listener)
    {
        var referenceDistance = _audio.GetAudioDistance(1f);
        var maxDistance = _audio.GetAudioDistance(stream.Range);
        var volume = _volume * _speakerVolume.GetValueOrDefault(stream.Speaker, 1f);
        if (stream.Route is VoiceRoute.Radio or VoiceRoute.RadioSpeaker)
            volume *= _radioVolume;

        volume = Math.Clamp(volume, 0f, 4f);
        var gain = MathF.Min(volume, 1f);
        var boost = MathF.Max(volume, 1f);

        if (stream.Global)
            return new VoicePlaybackParams(Vector2.Zero, gain, 0f, referenceDistance, maxDistance, boost, true, 1f);

        if (!TryGetEntity(stream.Source, out var uid) || TerminatingOrDeleted(uid.Value))
            return new VoicePlaybackParams(listener.Position, 0f, 0f, referenceDistance, maxDistance, boost, false, 0f);

        var coordinates = _transform.GetMapCoordinates(uid.Value);
        if (coordinates.MapId != listener.MapId)
            return new VoicePlaybackParams(listener.Position, 0f, 0f, referenceDistance, maxDistance, boost, false, 0f);

        var delta = coordinates.Position - listener.Position;
        var distance = delta.Length();
        var audioDistance = _audio.GetAudioDistance(distance);
        if (audioDistance > maxDistance)
            return new VoicePlaybackParams(coordinates.Position, 0f, 0f, referenceDistance, maxDistance, boost, false, 0f);

        var occlusion = distance > 0.1f ? _audio.GetOcclusion(listener, delta, distance, uid.Value) : 0f;
        var audibility = GetAudibility(audioDistance, referenceDistance, maxDistance, occlusion);
        return new VoicePlaybackParams(coordinates.Position, gain, occlusion, referenceDistance, maxDistance, boost, false, audibility);
    }

    private static float GetAudibility(float distance, float referenceDistance, float maxDistance, float occlusion)
    {
        var attenuation = maxDistance > referenceDistance
            ? 1f - (Math.Clamp(distance, referenceDistance, maxDistance) - referenceDistance) / (maxDistance - referenceDistance)
            : 1f;

        return attenuation * MathF.Exp(-occlusion * OcclusionVisibility);
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
