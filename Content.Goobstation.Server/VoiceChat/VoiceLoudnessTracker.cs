using Content.Goobstation.Shared.VoiceChat;

namespace Content.Goobstation.Server.VoiceChat;

public enum VoiceLoudness : byte
{
    Normal,
    Whisper,
    Shout,
}

public sealed class VoiceLoudnessTracker
{
    private const float FrameSeconds = VoiceCodec.FrameSamples / (float) VoiceCodec.SampleRate;
    private const float SilenceDb = -55f;
    private const float ShortTime = 0.5f;
    private const float BaselineTime = 15f;
    private const float WarmupTime = 1f;
    private const int WarmupFrames = 150;
    private const int ShoutHoldFrames = 75;
    private const int WhisperDelayFrames = 15;
    private const float Hysteresis = 3f;
    private const float DefaultBaselineDb = -30f;
    private const float MinBaselineDb = -50f;
    private const float MaxBaselineDb = -12f;

    private float _power;
    private bool _primed;
    private float _baseline = DefaultBaselineDb;
    private int _voicedFrames;
    private int _shoutHold;
    private int _quietFrames;

    public VoiceLoudness Mode { get; private set; }

    public void Reset()
    {
        _primed = false;
        _shoutHold = 0;
        _quietFrames = 0;
        Mode = VoiceLoudness.Normal;
    }

    public VoiceLoudness Update(float db, float shoutThreshold, float whisperThreshold)
    {
        if (db < SilenceDb)
            return Mode;

        var power = MathF.Pow(10f, db / 10f);
        if (_primed)
        {
            _power += (power - _power) * (FrameSeconds / ShortTime);
        }
        else
        {
            _power = power;
            _primed = true;
        }

        var shortDb = 10f * MathF.Log10(_power + 1e-12f);

        if (_voicedFrames < WarmupFrames)
        {
            _voicedFrames++;
            _baseline += (shortDb - _baseline) * (FrameSeconds / WarmupTime);
            Mode = VoiceLoudness.Normal;
            return Mode;
        }

        var relative = shortDb - _baseline;
        if (relative >= shoutThreshold)
        {
            Mode = VoiceLoudness.Shout;
            _shoutHold = ShoutHoldFrames;
            _quietFrames = 0;
        }
        else if (Mode == VoiceLoudness.Shout && _shoutHold > 0)
        {
            _shoutHold--;
        }
        else if (relative <= -whisperThreshold ||
                 Mode == VoiceLoudness.Whisper && relative <= Hysteresis - whisperThreshold)
        {
            _quietFrames++;
            Mode = Mode == VoiceLoudness.Whisper || _quietFrames >= WhisperDelayFrames
                ? VoiceLoudness.Whisper
                : VoiceLoudness.Normal;
        }
        else
        {
            Mode = VoiceLoudness.Normal;
            _quietFrames = 0;
        }

        if (Mode == VoiceLoudness.Normal)
        {
            _baseline = Math.Clamp(
                _baseline + (shortDb - _baseline) * (FrameSeconds / BaselineTime),
                MinBaselineDb,
                MaxBaselineDb);
        }

        return Mode;
    }
}
