namespace Content.Goobstation.Shared.VoiceChat;

public struct VoiceLevels
{
    public float Low;
    public float Mid;
    public float High;
    public float Overall;
}

public sealed class VoiceLevelAnalyzer
{
    private const float FloorDb = -48f;
    private const float RangeDb = 28f;
    private const float BandOffsetDb = 5f;
    private const float HighOffsetDb = 13f;

    private static readonly float LowCoefficient = 1f - MathF.Exp(-2f * MathF.PI * 400f / VoiceCodec.SampleRate);
    private static readonly float MidCoefficient = 1f - MathF.Exp(-2f * MathF.PI * 2000f / VoiceCodec.SampleRate);

    private float _low;
    private float _mid;

    public float LastDb { get; private set; } = float.NegativeInfinity;

    public VoiceLevels Analyze(ReadOnlySpan<short> samples)
    {
        if (samples.Length == 0)
        {
            LastDb = float.NegativeInfinity;
            return default;
        }

        var low = 0f;
        var mid = 0f;
        var high = 0f;
        var full = 0f;

        foreach (var value in samples)
        {
            var sample = value / 32768f;
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

        LastDb = ToDb(full, samples.Length);
        return new VoiceLevels
        {
            Low = ToLevel(low, samples.Length, BandOffsetDb),
            Mid = ToLevel(mid, samples.Length, BandOffsetDb),
            High = ToLevel(high, samples.Length, HighOffsetDb),
            Overall = ToLevel(full, samples.Length, 0f),
        };
    }

    private static float ToLevel(float energy, int count, float offsetDb)
    {
        var db = ToDb(energy, count) + offsetDb;
        return Math.Clamp((db - FloorDb) / RangeDb, 0f, 1f);
    }

    private static float ToDb(float energy, int count)
    {
        return 10f * MathF.Log10(energy / count + 1e-12f);
    }
}
