using Content.Goobstation.Shared.VoiceChat;
using NWaves.Effects;
using NWaves.Filters.BiQuad;

namespace Content.Goobstation.Server.VoiceChat;

public sealed class VoiceGodEffect
{
    public const int SilenceThreshold = 24;

    private const int SampleRate = VoiceCodec.SampleRate;
    private const int FftSize = 512;
    private const int HopSize = 128;

    private const float DryLevel = 0.5f;
    private const float VoiceLevel = 0.8f;
    private const float ShimmerLevel = 0.17f;
    private const float WetLevel = 0.36f;
    private const float LimiterKnee = 0.75f;

    private readonly PitchShiftVocoderEffect _pitchDown = new(SampleRate, 0.8, FftSize, HopSize);
    private readonly PitchShiftVocoderEffect _shimmer = new(SampleRate, 2.0, FftSize, HopSize);
    private readonly HighShelfFilter _air = new(4200.0 / SampleRate, 0.707, 3);
    private readonly LowPassFilter _shimmerTone = new(6000.0 / SampleRate, 0.707);
    private readonly Reverb _reverb = new();

    public VoiceGodEffect()
    {
        _pitchDown.Wet = 1f;
        _pitchDown.Dry = 0f;
        _shimmer.Wet = 1f;
        _shimmer.Dry = 0f;
    }

    public void Process(short[] buffer)
    {
        for (var i = 0; i < buffer.Length; i++)
        {
            var sample = buffer[i] / 32768f;
            var voice = _pitchDown.Process(sample) * VoiceLevel;
            var shimmer = _shimmerTone.Process(_shimmer.Process(sample)) * ShimmerLevel;
            var wet = _reverb.Process(voice + shimmer);
            var output = _air.Process(voice * DryLevel + wet * WetLevel);
            buffer[i] = (short) (Limit(output) * 32767f);
        }
    }

    public static bool IsSilent(short[] buffer)
    {
        foreach (var sample in buffer)
        {
            if (Math.Abs((int) sample) >= SilenceThreshold)
                return false;
        }

        return true;
    }

    private static float Limit(float sample)
    {
        var magnitude = MathF.Abs(sample);
        if (magnitude <= LimiterKnee)
            return sample;

        var limited = LimiterKnee + (1f - LimiterKnee) * MathF.Tanh((magnitude - LimiterKnee) / (1f - LimiterKnee));
        return MathF.CopySign(limited, sample);
    }

    private sealed class Reverb
    {
        private const float Feedback = 0.87f;
        private const float Damping = 0.42f;
        private const float AllPassFeedback = 0.5f;
        private const float InputGain = 0.02f;
        private const float EchoFeedback = 0.38f;
        private const float EchoDamping = 0.35f;
        private const float EchoSend = 0.7f;
        private const float EchoLevel = 0.16f;
        private const int PreDelay = SampleRate * 70 / 1000;
        private const int EchoDelay = SampleRate * 290 / 1000;

        private static readonly int[] CombLengths = { 1051, 1129, 1201, 1283, 1361, 1447, 1523, 1601 };
        private static readonly int[] AllPassLengths = { 347, 263, 202, 124 };

        private readonly float[][] _combs;
        private readonly float[] _combFilters;
        private readonly int[] _combIndices;
        private readonly float[][] _allPasses;
        private readonly int[] _allPassIndices;
        private readonly float[] _preDelay = new float[PreDelay];
        private readonly float[] _echo = new float[EchoDelay];
        private int _preDelayIndex;
        private int _echoIndex;
        private float _echoFilter;

        public Reverb()
        {
            _combs = new float[CombLengths.Length][];
            _combFilters = new float[CombLengths.Length];
            _combIndices = new int[CombLengths.Length];
            for (var i = 0; i < CombLengths.Length; i++)
            {
                _combs[i] = new float[CombLengths[i]];
            }

            _allPasses = new float[AllPassLengths.Length][];
            _allPassIndices = new int[AllPassLengths.Length];
            for (var i = 0; i < AllPassLengths.Length; i++)
            {
                _allPasses[i] = new float[AllPassLengths[i]];
            }
        }

        public float Process(float sample)
        {
            var delayed = _preDelay[_preDelayIndex];
            _preDelay[_preDelayIndex] = sample;
            _preDelayIndex = (_preDelayIndex + 1) % _preDelay.Length;

            var echo = _echo[_echoIndex];
            _echoFilter = echo * (1f - EchoDamping) + _echoFilter * EchoDamping;
            _echo[_echoIndex] = delayed + _echoFilter * EchoFeedback;
            _echoIndex = (_echoIndex + 1) % _echo.Length;

            var input = (delayed + echo * EchoSend) * InputGain;
            var output = 0f;
            for (var i = 0; i < _combs.Length; i++)
            {
                var buffer = _combs[i];
                var index = _combIndices[i];
                var value = buffer[index];
                _combFilters[i] = value * (1f - Damping) + _combFilters[i] * Damping;
                buffer[index] = input + _combFilters[i] * Feedback;
                _combIndices[i] = (index + 1) % buffer.Length;
                output += value;
            }

            for (var i = 0; i < _allPasses.Length; i++)
            {
                var buffer = _allPasses[i];
                var index = _allPassIndices[i];
                var stored = buffer[index];
                buffer[index] = output + stored * AllPassFeedback;
                _allPassIndices[i] = (index + 1) % buffer.Length;
                output = stored - output;
            }

            return output + echo * EchoLevel;
        }
    }
}
