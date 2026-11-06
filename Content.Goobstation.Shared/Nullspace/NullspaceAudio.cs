using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.Nullspace;

public static class NullspaceAudio
{
    public const string Reverb = "NullspaceCorridor";

    public static readonly SoundSpecifier HonkDistorted = new SoundPathSpecifier("/Audio/_Goobstation/Nullspace/honk_distorted.ogg");
    public static readonly SoundSpecifier Chime = new SoundPathSpecifier("/Audio/_Goobstation/Nullspace/windchime.ogg");
    public static readonly SoundSpecifier Stinger = new SoundCollectionSpecifier("NullspaceHorror");
    public static readonly SoundSpecifier LightOff = new SoundPathSpecifier("/Audio/_Goobstation/Nullspace/light_switch.ogg");
    public static readonly SoundSpecifier Buzz = new SoundPathSpecifier("/Audio/_Goobstation/Nullspace/buzz.ogg");
    public static readonly SoundSpecifier Sparks = new SoundPathSpecifier("/Audio/_Goobstation/Nullspace/door_sparks.ogg");
    public static readonly SoundSpecifier Step = new SoundCollectionSpecifier("NullspaceStep");
    public static readonly SoundSpecifier Run = new SoundCollectionSpecifier("NullspaceRun");
    public static readonly SoundSpecifier Breath = new SoundCollectionSpecifier("NullspaceBreath");
    public static readonly SoundSpecifier BreathHeavy = new SoundPathSpecifier("/Audio/_Goobstation/Nullspace/breath0.ogg");
    public static readonly SoundSpecifier Heartbeat = new SoundPathSpecifier("/Audio/_Goobstation/Nullspace/heartbeat.ogg");
    public static readonly SoundSpecifier Ambient = new SoundCollectionSpecifier("NullspaceAmbient");
    public static readonly SoundSpecifier Drone = new SoundPathSpecifier("/Audio/_Goobstation/Nullspace/low_drone.ogg");
    public static readonly SoundSpecifier Vent = new SoundPathSpecifier("/Audio/_Goobstation/Nullspace/ventilation.ogg");
    public static readonly SoundSpecifier Dread = new SoundPathSpecifier("/Audio/_Goobstation/Nullspace/the_dread.ogg");
    public static readonly SoundSpecifier Chase = new SoundPathSpecifier("/Audio/_Goobstation/Nullspace/chase.ogg");
    public static readonly SoundSpecifier Hum = new SoundPathSpecifier("/Audio/_Goobstation/Nullspace/hum.ogg");

    public static AudioParams Global(float gain, float pitch = 1f, bool loop = false)
    {
        return AudioParams.Default
            .WithVolume(SharedAudioSystem.GainToVolume(gain))
            .WithPitchScale(pitch)
            .WithLoop(loop);
    }

    public static AudioParams Positional(float gain, float pitch = 1f, float referenceMeters = 1.5f, float rolloff = 1f, float maxMeters = 40f, bool loop = false)
    {
        return AudioParams.Default
            .WithVolume(SharedAudioSystem.GainToVolume(gain))
            .WithPitchScale(pitch)
            .WithReferenceDistance(referenceMeters / NullspaceParams.Cell)
            .WithRolloffFactor(rolloff)
            .WithMaxDistance(maxMeters / NullspaceParams.Cell)
            .WithLoop(loop);
    }
}
