using System.Text;
using Content.Goobstation.Shared.VoiceChat;
using Robust.Shared.Network;

namespace Content.Goobstation.Server.VoiceChat;

public readonly record struct VoiceMixTarget(NetEntity Source, VoiceRoute Route, bool Global, float Range);

public sealed class VoiceRadioMixer(string channel, IServerNetManager net, Func<ushort> allocateStreamId)
{
    private static readonly TimeSpan FrameDuration = TimeSpan.FromMilliseconds(20);
    private static readonly TimeSpan StartDelay = TimeSpan.FromMilliseconds(60);
    private static readonly TimeSpan MaxLag = TimeSpan.FromMilliseconds(200);
    private static readonly TimeSpan ContributorTimeout = TimeSpan.FromMilliseconds(250);
    private static readonly TimeSpan VariantTimeout = TimeSpan.FromSeconds(2);
    private const int MaxQueuedFrames = 8;
    private const int PrimeFrames = 2;
    private const float LimiterKnee = 0.8f;

    private readonly Dictionary<ushort, Contributor> _contributors = new();
    private readonly Dictionary<string, Variant> _variants = new();
    private readonly Dictionary<INetChannel, VoiceMixTarget> _listeners = new();
    private readonly Dictionary<string, List<(INetChannel Listener, VoiceMixTarget Target)>> _groups = new();
    private readonly List<ushort> _expired = new();
    private readonly List<string> _staleVariants = new();
    private readonly Dictionary<VoiceMixTarget, List<INetChannel>> _targets = new();
    private readonly List<(ushort Speaker, short[] Pcm, byte Level)> _slot = new();
    private readonly List<ushort> _excluded = new();
    private readonly StringBuilder _key = new();
    private readonly float[] _mix = new float[VoiceCodec.FrameSamples];

    private TimeSpan? _nextFrame;

    public string Channel { get; } = channel;

    public bool Idle => _nextFrame == null && _variants.Count == 0;

    public void Push(ushort speaker, short[] pcm, byte level, Dictionary<INetChannel, VoiceMixTarget> recipients, TimeSpan now)
    {
        if (!_contributors.TryGetValue(speaker, out var contributor))
        {
            contributor = new Contributor();
            _contributors[speaker] = contributor;
        }

        contributor.Frames.Enqueue((pcm, level));
        while (contributor.Frames.Count > MaxQueuedFrames)
        {
            contributor.Frames.Dequeue();
        }

        contributor.Recipients = recipients;
        contributor.LastPush = now;

        _nextFrame ??= now + StartDelay;
    }

    public void Process(TimeSpan now, VoiceFormat format)
    {
        if (_nextFrame is { } next && now - next > MaxLag)
            _nextFrame = now - FrameDuration;

        while (_nextFrame is { } frameTime && frameTime <= now)
        {
            if (!MixFrame(now, format))
            {
                _nextFrame = null;
                foreach (var variant in _variants.Values)
                {
                    variant.Started = false;
                }

                break;
            }

            _nextFrame = frameTime + FrameDuration;
        }

        _staleVariants.Clear();
        foreach (var (key, variant) in _variants)
        {
            if (now - variant.LastUsed > VariantTimeout)
                _staleVariants.Add(key);
        }

        foreach (var key in _staleVariants)
        {
            _variants.Remove(key);
            _groups.Remove(key);
        }
    }

    private bool MixFrame(TimeSpan now, VoiceFormat format)
    {
        _slot.Clear();
        _expired.Clear();
        foreach (var (speaker, contributor) in _contributors)
        {
            if (!contributor.Primed && contributor.Frames.Count >= PrimeFrames)
                contributor.Primed = true;

            if (contributor.Primed && contributor.Frames.TryDequeue(out var frame))
            {
                _slot.Add((speaker, frame.Pcm, frame.Level));
                continue;
            }

            if (now - contributor.LastPush > ContributorTimeout)
                _expired.Add(speaker);
        }

        foreach (var speaker in _expired)
        {
            _contributors.Remove(speaker);
        }

        if (_contributors.Count == 0)
            return false;

        _listeners.Clear();
        foreach (var contributor in _contributors.Values)
        {
            foreach (var (listener, target) in contributor.Recipients)
            {
                _listeners.TryAdd(listener, target);
            }
        }

        foreach (var list in _groups.Values)
        {
            list.Clear();
        }

        foreach (var (listener, target) in _listeners)
        {
            var key = BuildExclusionKey(listener);
            if (!_groups.TryGetValue(key, out var list))
            {
                list = new List<(INetChannel, VoiceMixTarget)>();
                _groups[key] = list;
            }

            list.Add((listener, target));
        }

        foreach (var (key, listeners) in _groups)
        {
            if (listeners.Count == 0)
                continue;

            if (!_variants.TryGetValue(key, out var variant))
            {
                variant = new Variant(allocateStreamId());
                _variants[key] = variant;
            }

            SendVariant(variant, listeners, format, now);
        }

        return true;
    }

    private string BuildExclusionKey(INetChannel listener)
    {
        _excluded.Clear();
        foreach (var (speaker, contributor) in _contributors)
        {
            if (!contributor.Recipients.ContainsKey(listener))
                _excluded.Add(speaker);
        }

        if (_excluded.Count == 0)
            return string.Empty;

        _excluded.Sort();
        _key.Clear();
        foreach (var speaker in _excluded)
        {
            _key.Append(speaker).Append(',');
        }

        return _key.ToString();
    }

    private void SendVariant(Variant variant, List<(INetChannel Listener, VoiceMixTarget Target)> listeners, VoiceFormat format, TimeSpan now)
    {
        Array.Clear(_mix);
        var contributors = new List<VoiceContributor>();
        var listener = listeners[0].Listener;
        foreach (var (speaker, pcm, level) in _slot)
        {
            if (!_contributors.TryGetValue(speaker, out var contributor) || !contributor.Recipients.ContainsKey(listener))
                continue;

            for (var i = 0; i < _mix.Length; i++)
            {
                _mix[i] += pcm[i] / 32768f;
            }

            contributors.Add(new VoiceContributor(speaker, level));
        }

        for (var i = 0; i < _mix.Length; i++)
        {
            variant.Pcm[i] = (short) (Limit(_mix[i]) * 32767f);
        }

        variant.Effects.Process(variant.Pcm, 0, variant.Pcm.Length, new VoiceEffectSettings(VoiceEffect.Radio, VoiceMuffle.None));
        var payload = variant.Encoder.Encode(variant.Pcm, format);
        var flags = variant.Started ? (byte) 0 : VoiceCodec.FlagTransmissionStart;
        variant.Started = true;
        variant.LastUsed = now;

        foreach (var list in _targets.Values)
        {
            list.Clear();
        }

        foreach (var (recipient, target) in listeners)
        {
            if (variant.InfoSent.Add(recipient))
            {
                net.ServerSendMessage(new MsgVoiceSpeakerInfo
                {
                    Speaker = variant.StreamId,
                    Name = string.Empty,
                    Channel = Channel,
                }, recipient);
            }

            if (!_targets.TryGetValue(target, out var recipients))
            {
                recipients = new List<INetChannel>();
                _targets[target] = recipients;
            }

            recipients.Add(recipient);
        }

        foreach (var (target, recipients) in _targets)
        {
            if (recipients.Count == 0)
                continue;

            net.ServerSendToMany(new MsgVoiceFrame
            {
                Source = target.Source,
                Speaker = variant.StreamId,
                Sequence = variant.Sequence,
                Flags = flags,
                Route = target.Route,
                Format = format,
                Global = target.Global,
                Range = target.Range,
                Payload = payload,
                Contributors = contributors,
            }, recipients);
        }

        if (_targets.Count > 64)
            _targets.Clear();

        variant.Sequence++;
    }

    private static float Limit(float sample)
    {
        var magnitude = MathF.Abs(sample);
        if (magnitude <= LimiterKnee)
            return sample;

        var limited = LimiterKnee + (1f - LimiterKnee) * MathF.Tanh((magnitude - LimiterKnee) / (1f - LimiterKnee));
        return MathF.CopySign(limited, sample);
    }

    private sealed class Contributor
    {
        public readonly Queue<(short[] Pcm, byte Level)> Frames = new();
        public Dictionary<INetChannel, VoiceMixTarget> Recipients = new();
        public TimeSpan LastPush;
        public bool Primed;
    }

    private sealed class Variant(ushort streamId)
    {
        public readonly ushort StreamId = streamId;
        public readonly short[] Pcm = new short[VoiceCodec.FrameSamples];
        public readonly VoiceEffectProcessor Effects = new();
        public readonly VoiceEncoder Encoder = new();
        public readonly HashSet<INetChannel> InfoSent = new();
        public ushort Sequence;
        public bool Started;
        public TimeSpan LastUsed;
    }
}
