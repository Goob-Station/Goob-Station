namespace Content.Goobstation.Shared.VoiceChat;

public static class VoiceCodec
{
    public const int SampleRate = 16000;
    public const int FrameSamples = 320;
    public const int HeaderBytes = 3;
    public const int FrameBytes = HeaderBytes + FrameSamples / 2;

    public const byte FlagTransmissionStart = 1;
    public const int StepCount = 89;

    private static readonly int[] IndexTable =
    {
        -1, -1, -1, -1, 2, 4, 6, 8,
        -1, -1, -1, -1, 2, 4, 6, 8,
    };

    private static readonly int[] StepTable =
    {
        7, 8, 9, 10, 11, 12, 13, 14, 16, 17,
        19, 21, 23, 25, 28, 31, 34, 37, 41, 45,
        50, 55, 60, 66, 73, 80, 88, 97, 107, 118,
        130, 143, 157, 173, 190, 209, 230, 253, 279, 307,
        337, 371, 408, 449, 494, 544, 598, 658, 724, 796,
        876, 963, 1060, 1166, 1282, 1411, 1552, 1707, 1878, 2066,
        2272, 2499, 2749, 3024, 3327, 3660, 4026, 4428, 4871, 5358,
        5894, 6484, 7132, 7845, 8630, 9493, 10442, 11487, 12635, 13899,
        15289, 16818, 18500, 20350, 22385, 24623, 27086, 29794, 32767,
    };

    public static int Step(int index)
    {
        return StepTable[index];
    }

    public static bool IsValidFrame(ReadOnlySpan<byte> frame)
    {
        return frame.Length == FrameBytes && frame[2] < StepTable.Length;
    }

    public static void Encode(ReadOnlySpan<short> samples, ref int predictor, ref int index, Span<byte> output)
    {
        output[0] = (byte) (predictor & 0xFF);
        output[1] = (byte) ((predictor >> 8) & 0xFF);
        output[2] = (byte) index;
        for (var i = HeaderBytes; i < FrameBytes; i++)
        {
            output[i] = 0;
        }

        for (var i = 0; i < FrameSamples; i++)
        {
            var step = StepTable[index];
            var diff = samples[i] - predictor;
            var nibble = 0;
            if (diff < 0)
            {
                nibble = 8;
                diff = -diff;
            }

            var delta = step >> 3;
            if (diff >= step)
            {
                nibble |= 4;
                diff -= step;
                delta += step;
            }

            step >>= 1;
            if (diff >= step)
            {
                nibble |= 2;
                diff -= step;
                delta += step;
            }

            step >>= 1;
            if (diff >= step)
            {
                nibble |= 1;
                delta += step;
            }

            predictor = (nibble & 8) != 0 ? predictor - delta : predictor + delta;
            predictor = Math.Clamp(predictor, short.MinValue, short.MaxValue);
            index = Math.Clamp(index + IndexTable[nibble], 0, StepTable.Length - 1);

            output[HeaderBytes + (i >> 1)] |= (byte) ((i & 1) == 0 ? nibble : nibble << 4);
        }
    }

    public static bool Decode(ReadOnlySpan<byte> frame, Span<short> output)
    {
        if (!IsValidFrame(frame) || output.Length < FrameSamples)
            return false;

        int predictor = (short) (frame[0] | (frame[1] << 8));
        int index = frame[2];

        for (var i = 0; i < FrameSamples; i++)
        {
            var packed = frame[HeaderBytes + (i >> 1)];
            var nibble = (i & 1) == 0 ? packed & 0x0F : packed >> 4;

            var step = StepTable[index];
            var diff = step >> 3;
            if ((nibble & 1) != 0)
                diff += step >> 2;
            if ((nibble & 2) != 0)
                diff += step >> 1;
            if ((nibble & 4) != 0)
                diff += step;

            predictor = (nibble & 8) != 0 ? predictor - diff : predictor + diff;
            predictor = Math.Clamp(predictor, short.MinValue, short.MaxValue);
            index = Math.Clamp(index + IndexTable[nibble], 0, StepTable.Length - 1);

            output[i] = (short) predictor;
        }

        return true;
    }
}
