using System.Numerics;
using Content.Goobstation.Shared.VoiceChat;
using Robust.Client.Audio;
using Robust.Shared.Audio.Sources;

namespace Content.Goobstation.Client.VoiceChat;

public readonly record struct VoicePlaybackParams(
    Vector2 Position,
    float Gain,
    float Occlusion,
    float ReferenceDistance,
    float MaxDistance,
    float Boost);

public sealed class VoicePlaybackStream : IDisposable
{
    private const int SampleRate = VoiceCodec.SampleRate;
    private const int FrameSamples = VoiceCodec.FrameSamples;
    private const int RampSamples = SampleRate * 5 / 1000;
    private const int HoldSamples = SampleRate * 45 / 1000;
    private const int OverlapSamples = HoldSamples + RampSamples;
    private const int LinkableSamples = OverlapSamples + RampSamples;
    private const int GapFadeSamples = 64;
    private const int StartSamples = SampleRate * 80 / 1000;
    private const int TargetSamples = SampleRate * 120 / 1000;
    private const int MaxLatencySamples = SampleRate * 250 / 1000;
    private const int MaxBufferedSamples = SampleRate * 5;
    private const int ConcealAfterFrames = 3;
    private const int MaxSequenceJump = 50;

    private static readonly TimeSpan StartWait = TimeSpan.FromMilliseconds(60);
    private static readonly TimeSpan SequenceResetAfter = TimeSpan.FromSeconds(1);

    private readonly IAudioManager _audioManager;
    private readonly Dictionary<ushort, short[]> _pending = new();
    private readonly List<Chunk> _playing = new();

    private short[] _pcm = new short[SampleRate];
    private short[] _chunkBuffer = new short[SampleRate];
    private long _pcmStart;
    private int _pcmLength;
    private long _scheduledUntil;
    private ushort _nextSequence;
    private bool _hasSequence;
    private bool _fadeInNext = true;
    private short[]? _lastFrame;
    private TimeSpan _lastReceived;
    private Chunk? _current;
    private VoicePlaybackParams _params;

    public NetEntity Source;
    public TimeSpan LastActivity;

    public VoicePlaybackStream(IAudioManager audioManager)
    {
        _audioManager = audioManager;
    }

    private long PcmEnd => _pcmStart + _pcmLength;

    public bool IsIdle => _playing.Count == 0 && PcmEnd <= _scheduledUntil && _pending.Count == 0;

    public void AddFrame(ushort sequence, byte flags, short[] pcm, TimeSpan now)
    {
        LastActivity = now;

        var transmissionStart = (flags & VoiceCodec.FlagTransmissionStart) != 0;
        if (!_hasSequence ||
            now - _lastReceived > SequenceResetAfter ||
            transmissionStart && sequence != _nextSequence ||
            Math.Abs(SequenceDelta(sequence, _nextSequence)) > MaxSequenceJump)
        {
            _pending.Clear();
            _nextSequence = sequence;
            _hasSequence = true;
            _fadeInNext = true;
        }

        _lastReceived = now;

        if (SequenceDelta(sequence, _nextSequence) < 0)
            return;

        _pending[sequence] = pcm;
        DrainPending();
    }

    public void Update(TimeSpan now, VoicePlaybackParams parameters)
    {
        _params = parameters;

        RetireFinished();

        foreach (var chunk in _playing)
        {
            Apply(chunk.Source);
        }

        Schedule(now);
        Trim();
    }

    public void Dispose()
    {
        foreach (var chunk in _playing)
        {
            chunk.Dispose();
        }

        _playing.Clear();
        _current = null;
    }

    private void Schedule(TimeSpan now)
    {
        if (_current is { Disposed: true })
            _current = null;

        if (_current == null)
        {
            StartFresh(now);
            return;
        }

        if (!_current.Linkable || PcmEnd - _current.End < FrameSamples)
            return;

        var position = _current.Start + (long) MathF.Round(_current.Source.PlaybackPosition * SampleRate);
        var contentStart = _current.End - OverlapSamples;
        var offset = position - contentStart;
        if (offset is < 0 or >= HoldSamples)
            return;

        var ahead = PcmEnd - position;
        var skip = ahead > MaxLatencySamples ? ahead - TargetSamples : 0;

        var chunk = CreateChunk(contentStart + skip, PcmEnd, true);
        if (chunk == null)
            return;

        Play(chunk, offset);
    }

    private void StartFresh(TimeSpan now)
    {
        var start = Math.Max(_scheduledUntil, _pcmStart);
        var available = PcmEnd - start;
        if (available <= 0)
            return;

        if (available < StartSamples && now - _lastReceived < StartWait)
            return;

        if (available > MaxLatencySamples)
            start = PcmEnd - TargetSamples;

        var chunk = CreateChunk(start, PcmEnd, false);
        if (chunk != null)
            Play(chunk, 0);
    }

    private void Play(Chunk chunk, long offsetSamples)
    {
        Apply(chunk.Source);
        chunk.Source.RolloffFactor = 1f;
        chunk.Source.ReferenceDistance = _params.ReferenceDistance;
        chunk.Source.MaxDistance = _params.MaxDistance;

        if (offsetSamples > 0)
            chunk.Source.PlaybackPosition = offsetSamples / (float) SampleRate;

        chunk.Source.StartPlaying();

        _playing.Add(chunk);
        _current = chunk;
        _scheduledUntil = chunk.End;
    }

    private Chunk? CreateChunk(long start, long end, bool holdHead)
    {
        var length = (int) (end - start);
        if (length <= 0 || start < _pcmStart || end > PcmEnd)
            return null;

        if (_chunkBuffer.Length < length)
            _chunkBuffer = new short[Math.Max(length, _chunkBuffer.Length * 2)];

        var boost = _params.Boost;
        var offset = (int) (start - _pcmStart);
        for (var i = 0; i < length; i++)
        {
            var gain = HeadGain(i, holdHead) * TailGain(i, length) * boost;
            var value = _pcm[offset + i] * gain;
            _chunkBuffer[i] = (short) Math.Clamp(value, short.MinValue, short.MaxValue);
        }

        var stream = _audioManager.LoadAudioRaw(new ReadOnlySpan<short>(_chunkBuffer, 0, length), 1, SampleRate);
        var source = _audioManager.CreateAudioSource(stream);
        if (source == null)
        {
            stream.Dispose();
            return null;
        }

        return new Chunk(stream, source, start, end, length >= LinkableSamples);
    }

    private static float HeadGain(int index, bool hold)
    {
        if (hold)
        {
            if (index < HoldSamples)
                return 0f;

            index -= HoldSamples;
        }

        return index < RampSamples ? (index + 0.5f) / RampSamples : 1f;
    }

    private static float TailGain(int index, int length)
    {
        var fromEnd = index - (length - RampSamples);
        return fromEnd < 0 ? 1f : 1f - (fromEnd + 0.5f) / RampSamples;
    }

    private void Apply(IAudioSource source)
    {
        source.Position = _params.Position;
        source.Gain = _params.Gain;
        source.Occlusion = _params.Occlusion;
    }

    private void RetireFinished()
    {
        for (var i = _playing.Count - 1; i >= 0; i--)
        {
            var chunk = _playing[i];
            if (chunk.Source.Playing)
                continue;

            chunk.Dispose();
            _playing.RemoveAt(i);
        }
    }

    private void DrainPending()
    {
        while (true)
        {
            if (_pending.Remove(_nextSequence, out var frame))
            {
                Append(frame);
                _nextSequence++;
                continue;
            }

            if (_pending.Count < ConcealAfterFrames)
                return;

            Append(Conceal());
            _nextSequence++;
            _fadeInNext = true;
        }
    }

    private short[] Conceal()
    {
        var concealed = new short[FrameSamples];
        if (_lastFrame == null)
            return concealed;

        for (var i = 0; i < FrameSamples; i++)
        {
            concealed[i] = (short) (_lastFrame[FrameSamples - 1 - i] * (1f - (i + 1f) / FrameSamples));
        }

        return concealed;
    }

    private void Append(short[] frame)
    {
        if (_pcm.Length < _pcmLength + FrameSamples)
            Array.Resize(ref _pcm, Math.Max(_pcmLength + FrameSamples, _pcm.Length * 2));

        Array.Copy(frame, 0, _pcm, _pcmLength, FrameSamples);

        if (_fadeInNext)
        {
            for (var i = 0; i < GapFadeSamples; i++)
            {
                _pcm[_pcmLength + i] = (short) (_pcm[_pcmLength + i] * (i / (float) GapFadeSamples));
            }

            _fadeInNext = false;
        }

        _pcmLength += FrameSamples;
        _lastFrame = frame;
    }

    private void Trim()
    {
        var keepFrom = _current is { Disposed: false } current
            ? current.End - OverlapSamples
            : _scheduledUntil;

        keepFrom = Math.Max(keepFrom, PcmEnd - MaxBufferedSamples);
        keepFrom = Math.Clamp(keepFrom, _pcmStart, PcmEnd);

        var discard = (int) (keepFrom - _pcmStart);
        if (discard < SampleRate / 2 && _pcmLength < MaxBufferedSamples)
            return;

        _pcmLength -= discard;
        Array.Copy(_pcm, discard, _pcm, 0, _pcmLength);
        _pcmStart = keepFrom;
    }

    private static int SequenceDelta(ushort a, ushort b)
    {
        return (short) (a - b);
    }

    private sealed class Chunk(AudioStream stream, IAudioSource source, long start, long end, bool linkable) : IDisposable
    {
        public readonly AudioStream Stream = stream;
        public readonly IAudioSource Source = source;
        public readonly long Start = start;
        public readonly long End = end;
        public readonly bool Linkable = linkable;
        public bool Disposed;

        public void Dispose()
        {
            if (Disposed)
                return;

            Disposed = true;
            Source.Dispose();
            Stream.Dispose();
        }
    }
}
