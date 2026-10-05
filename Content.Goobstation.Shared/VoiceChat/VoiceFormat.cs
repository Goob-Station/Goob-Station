namespace Content.Goobstation.Shared.VoiceChat;

public enum VoiceFormat : byte
{
    Wide4,
    Wide3,
    Narrow4,
    Narrow3,
    Narrow2,
}

public static class VoiceFormats
{
    private static readonly int[] IndexTable4 = { -1, -1, -1, -1, 2, 4, 6, 8 };
    private static readonly int[] IndexTable3 = { -1, -1, 1, 2 };
    private static readonly int[] IndexTable2 = { -1, 2 };

    public static VoiceFormat FromBitrate(int kbps)
    {
        return kbps switch
        {
            >= 56 => VoiceFormat.Wide4,
            >= 40 => VoiceFormat.Wide3,
            >= 28 => VoiceFormat.Narrow4,
            >= 20 => VoiceFormat.Narrow3,
            _ => VoiceFormat.Narrow2,
        };
    }

    public static bool IsNarrow(VoiceFormat format)
    {
        return format is VoiceFormat.Narrow4 or VoiceFormat.Narrow3 or VoiceFormat.Narrow2;
    }

    public static int Bits(VoiceFormat format)
    {
        return format switch
        {
            VoiceFormat.Wide4 or VoiceFormat.Narrow4 => 4,
            VoiceFormat.Wide3 or VoiceFormat.Narrow3 => 3,
            _ => 2,
        };
    }

    public static int Samples(VoiceFormat format)
    {
        return IsNarrow(format) ? VoiceCodec.FrameSamples / 2 : VoiceCodec.FrameSamples;
    }

    public static int PayloadBytes(VoiceFormat format)
    {
        return VoiceCodec.HeaderBytes + (Samples(format) * Bits(format) + 7) / 8;
    }

    public static bool IsValid(ReadOnlySpan<byte> payload, VoiceFormat format)
    {
        return format <= VoiceFormat.Narrow2 &&
               payload.Length == PayloadBytes(format) &&
               payload[2] < VoiceCodec.StepCount;
    }

    public static void Encode(ReadOnlySpan<short> samples, int bits, ref int predictor, ref int index, Span<byte> output)
    {
        output[0] = (byte) (predictor & 0xFF);
        output[1] = (byte) ((predictor >> 8) & 0xFF);
        output[2] = (byte) index;
        output[VoiceCodec.HeaderBytes..].Clear();

        var magnitudeBits = bits - 1;
        var signBit = 1 << magnitudeBits;
        var indexTable = IndexTableFor(bits);
        var bitPosition = 0;

        foreach (var sample in samples)
        {
            var step = VoiceCodec.Step(index);
            var diff = sample - predictor;
            var code = 0;
            if (diff < 0)
            {
                code = signBit;
                diff = -diff;
            }

            var delta = step >> magnitudeBits;
            var threshold = step;
            for (var bit = magnitudeBits - 1; bit >= 0; bit--)
            {
                if (diff >= threshold)
                {
                    code |= 1 << bit;
                    diff -= threshold;
                    delta += threshold;
                }

                threshold >>= 1;
            }

            predictor = Math.Clamp((code & signBit) != 0 ? predictor - delta : predictor + delta, short.MinValue, short.MaxValue);
            index = Math.Clamp(index + indexTable[code & (signBit - 1)], 0, VoiceCodec.StepCount - 1);

            var offset = VoiceCodec.HeaderBytes + (bitPosition >> 3);
            var shifted = code << (bitPosition & 7);
            output[offset] |= (byte) shifted;
            if ((bitPosition & 7) + bits > 8)
                output[offset + 1] |= (byte) (shifted >> 8);

            bitPosition += bits;
        }
    }

    public static void Decode(ReadOnlySpan<byte> payload, int bits, Span<short> output)
    {
        int predictor = (short) (payload[0] | (payload[1] << 8));
        int index = payload[2];

        var magnitudeBits = bits - 1;
        var signBit = 1 << magnitudeBits;
        var mask = (1 << bits) - 1;
        var indexTable = IndexTableFor(bits);
        var bitPosition = 0;

        for (var i = 0; i < output.Length; i++)
        {
            var offset = VoiceCodec.HeaderBytes + (bitPosition >> 3);
            var shift = bitPosition & 7;
            var value = payload[offset] >> shift;
            if (shift + bits > 8)
                value |= payload[offset + 1] << (8 - shift);

            var code = value & mask;
            bitPosition += bits;

            var step = VoiceCodec.Step(index);
            var delta = step >> magnitudeBits;
            var threshold = step;
            for (var bit = magnitudeBits - 1; bit >= 0; bit--)
            {
                if ((code & (1 << bit)) != 0)
                    delta += threshold;

                threshold >>= 1;
            }

            predictor = Math.Clamp((code & signBit) != 0 ? predictor - delta : predictor + delta, short.MinValue, short.MaxValue);
            index = Math.Clamp(index + indexTable[code & (signBit - 1)], 0, VoiceCodec.StepCount - 1);
            output[i] = (short) predictor;
        }
    }

    private static int[] IndexTableFor(int bits)
    {
        return bits switch
        {
            4 => IndexTable4,
            3 => IndexTable3,
            _ => IndexTable2,
        };
    }
}

public sealed class VoiceResampler
{
    private const int TapCount = 31;
    private const float Cutoff = 3700f / VoiceCodec.SampleRate;

    private static readonly float[] Taps = BuildTaps();

    private readonly float[] _buffer = new float[TapCount - 1 + VoiceCodec.FrameSamples];

    public void Downsample(ReadOnlySpan<short> input, Span<short> output)
    {
        Load(input, 1f, false);
        for (var i = 0; i < output.Length; i++)
        {
            output[i] = Filter(i * 2 + 1);
        }

        Shift(input.Length);
    }

    public void Upsample(ReadOnlySpan<short> input, Span<short> output)
    {
        Load(input, 2f, true);
        for (var i = 0; i < output.Length; i++)
        {
            output[i] = Filter(i);
        }

        Shift(input.Length * 2);
    }

    private void Load(ReadOnlySpan<short> input, float gain, bool stuff)
    {
        var start = TapCount - 1;
        if (stuff)
        {
            for (var i = 0; i < input.Length; i++)
            {
                _buffer[start + i * 2] = input[i] * gain;
                _buffer[start + i * 2 + 1] = 0f;
            }

            return;
        }

        for (var i = 0; i < input.Length; i++)
        {
            _buffer[start + i] = input[i] * gain;
        }
    }

    private short Filter(int position)
    {
        var sum = 0f;
        for (var tap = 0; tap < TapCount; tap++)
        {
            sum += Taps[tap] * _buffer[position + tap];
        }

        return (short) Math.Clamp(sum, short.MinValue, short.MaxValue);
    }

    private void Shift(int consumed)
    {
        Array.Copy(_buffer, consumed, _buffer, 0, TapCount - 1);
    }

    private static float[] BuildTaps()
    {
        var taps = new float[TapCount];
        var middle = (TapCount - 1) / 2f;
        var sum = 0f;
        for (var i = 0; i < TapCount; i++)
        {
            var x = i - middle;
            var sinc = x == 0f ? 2f * Cutoff : MathF.Sin(2f * MathF.PI * Cutoff * x) / (MathF.PI * x);
            var window = 0.42f - 0.5f * MathF.Cos(2f * MathF.PI * i / (TapCount - 1)) + 0.08f * MathF.Cos(4f * MathF.PI * i / (TapCount - 1));
            taps[i] = sinc * window;
            sum += taps[i];
        }

        for (var i = 0; i < TapCount; i++)
        {
            taps[i] /= sum;
        }

        return taps;
    }
}

public sealed class VoiceEncoder
{
    private readonly VoiceResampler _resampler = new();
    private readonly short[] _narrow = new short[VoiceCodec.FrameSamples / 2];
    private int _predictor;
    private int _index;

    public byte[] Encode(ReadOnlySpan<short> samples, VoiceFormat format)
    {
        var output = new byte[VoiceFormats.PayloadBytes(format)];
        if (VoiceFormats.IsNarrow(format))
        {
            _resampler.Downsample(samples, _narrow);
            samples = _narrow;
        }

        VoiceFormats.Encode(samples, VoiceFormats.Bits(format), ref _predictor, ref _index, output);
        return output;
    }
}

public sealed class VoiceDecoder
{
    private readonly VoiceResampler _resampler = new();
    private readonly short[] _narrow = new short[VoiceCodec.FrameSamples / 2];

    public bool Decode(ReadOnlySpan<byte> payload, VoiceFormat format, Span<short> output)
    {
        if (!VoiceFormats.IsValid(payload, format) || output.Length < VoiceCodec.FrameSamples)
            return false;

        if (!VoiceFormats.IsNarrow(format))
        {
            VoiceFormats.Decode(payload, VoiceFormats.Bits(format), output[..VoiceCodec.FrameSamples]);
            return true;
        }

        VoiceFormats.Decode(payload, VoiceFormats.Bits(format), _narrow);
        _resampler.Upsample(_narrow, output[..VoiceCodec.FrameSamples]);
        return true;
    }
}
