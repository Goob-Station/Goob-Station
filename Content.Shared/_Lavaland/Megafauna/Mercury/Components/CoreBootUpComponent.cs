using Robust.Shared.Audio;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Components;

/// <summary>
/// This component is added to an entity that is in the process of booting up for visual flare.
/// Increases brightness, emits a sound, and optionally.
/// </summary>

[RegisterComponent]
public sealed partial class CoreBootUpComponent : Component
{
    /// <summary>
    /// How brightly the entity should glow. Serves as a cap.
    /// </summary>
    [DataField]
    public float GlowIntensity = 5;

    /// <summary>
    /// The current glow of the entity, used in tandem with GlowIntensity as a cap.
    /// </summary>
    public float CurrentGlow;

    /// <summary>
    /// How rapidly the glow increaes per tick.
    /// </summary>
    [DataField]
    public float IncreaseBy = 0.1f;

    /// <summary>
    /// Used for reversing the logic.
    /// </summary>
    public bool ReduceGlow;

    /// <summary>
    /// Used to prevent sound spam.
    /// </summary>
    public bool SoundPlayed;

    /// <summary>
    /// The sound it makes when glowing brighter.
    /// </summary>
    [DataField]
    public SoundSpecifier BootUpSound = new SoundPathSpecifier("/Audio/_Goobstation/Ambience/ominous_pulse.ogg");

    public TimeSpan NextTick;

    [DataField]
    public TimeSpan Interval;

    [DataField]
    public Color LightColor = Color.Cyan;
}
