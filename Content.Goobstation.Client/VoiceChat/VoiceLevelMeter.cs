using Content.Goobstation.Shared.VoiceChat;

namespace Content.Goobstation.Client.VoiceChat;

public struct VoiceLevels
{
    public float Low;
    public float Mid;
    public float High;
    public float Overall;
}

public sealed class VoiceLevelMeter
{
    public const int BlockSamples = VoiceCodec.SampleRate / 100;

    private const int Capacity = 512;
    private const float FloorDb = -48f;
    private const float RangeDb = 28f;
    private const float BandOffsetDb = 5f;
    private const float HighOffsetDb = 13f;

    private static readonly float LowCoefficient = 1f - MathF.Exp(-2f * MathF.PI * 400f / VoiceCodec.SampleRate);
    private static readonly float MidCoefficient = 1f - MathF.Exp(-2f * MathF.PI * 2000f / VoiceCodec.SampleRate);

    private readonly long[] _blocks = new long[Capacity];
    private readonly VoiceLevels[] _levels = new VoiceLevels[Capacity];
    private float _low;
    private float _mid;

    public void Analyze(short[] pcm, int offset, int count, long position)
    {
        for (var start = 0; start + BlockSamples <= count; start += BlockSamples)
        {
            var low = 0f;
            var mid = 0f;
            var high = 0f;
            var full = 0f;

            for (var i = 0; i < BlockSamples; i++)
            {
                var sample = pcm[offset + start + i] / 32768f;
                _low += (sample - _low) * LowCoefficient;
                _mid += (sample - _mid) * MidCoefficient;

                var lowBand = _low;
                var midBand = _mid - _low;
                var highBand = sample - _mid;
                low += lowBand * lowBand;
                mid += midBand * midBand;
                high += highBand * highBand;
                full += sample * sample;
            }

            var block = (position + start) / BlockSamples;
            var slot = (int) (block % Capacity);
            _blocks[slot] = block;
            _levels[slot] = new VoiceLevels
            {
                Low = ToLevel(low, BandOffsetDb),
                Mid = ToLevel(mid, BandOffsetDb),
                High = ToLevel(high, HighOffsetDb),
                Overall = ToLevel(full, 0f),
            };
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

    private static float ToLevel(float energy, float offsetDb)
    {
        var db = 10f * MathF.Log10(energy / BlockSamples + 1e-12f) + offsetDb;
        return Math.Clamp((db - FloorDb) / RangeDb, 0f, 1f);
    }
}
