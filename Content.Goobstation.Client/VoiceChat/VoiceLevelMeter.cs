using Content.Goobstation.Shared.VoiceChat;

namespace Content.Goobstation.Client.VoiceChat;

public sealed class VoiceLevelMeter
{
    public const int BlockSamples = VoiceCodec.SampleRate / 100;

    private const int Capacity = 512;

    private readonly VoiceLevelAnalyzer _analyzer = new();
    private readonly long[] _blocks = new long[Capacity];
    private readonly VoiceLevels[] _levels = new VoiceLevels[Capacity];

    public void Analyze(short[] pcm, int offset, int count, long position)
    {
        for (var start = 0; start + BlockSamples <= count; start += BlockSamples)
        {
            var block = (position + start) / BlockSamples;
            var slot = (int) (block % Capacity);
            _blocks[slot] = block;
            _levels[slot] = _analyzer.Analyze(new ReadOnlySpan<short>(pcm, offset + start, BlockSamples));
        }
    }

    public VoiceLevels Get(long position)
    {
        if (position < 0)
            return default;

        var block = position / BlockSamples;
        var slot = (int) (block % Capacity);
        return _blocks[slot] == block ? _levels[slot] : default;
    }
}

public static class VoiceLevelSmoothing
{
    private const float AttackTime = 0.025f;
    private const float ReleaseTime = 0.15f;

    public static void Apply(ref VoiceLevels current, VoiceLevels target, float frameTime)
    {
        var attack = 1f - MathF.Exp(-frameTime / AttackTime);
        var release = 1f - MathF.Exp(-frameTime / ReleaseTime);
        current.Low = Smooth(current.Low, target.Low, attack, release);
        current.Mid = Smooth(current.Mid, target.Mid, attack, release);
        current.High = Smooth(current.High, target.High, attack, release);
        current.Overall = Smooth(current.Overall, target.Overall, attack, release);
    }

    private static float Smooth(float current, float target, float attack, float release)
    {
        return current + (target - current) * (target > current ? attack : release);
    }
}
