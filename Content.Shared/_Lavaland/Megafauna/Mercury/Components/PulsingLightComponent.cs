using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Components;

/// <summary>
/// This component can be added to an entity to give the appearance of a pulsing light with sound.
/// Sound is optional.
/// </summary>

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PulsingLightComponent : Component
{
    /// <summary>
    /// How brightly the entity should glow. Serves as a cap.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float GlowIntensity = 5;

    /// <summary>
    /// The current glow of the entity, used in tandem with GlowIntensity as a cap.
    /// </summary>
    public float CurrentGlow;

    /// <summary>
    /// How rapidly the glow increaes per tick.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float IncreaseBy = 0.1f;

    /// <summary>
    /// Used for reversing the logic.
    /// </summary>
    public bool ReduceGlow;

    /// <summary>
    /// If true, plays sound.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ShouldPlaySound;

    /// <summary>
    /// Used to prevent sound spam.
    /// </summary>
    public bool SoundPlayed;

    /// <summary>
    /// The sound it makes when glowing brighter.
    /// </summary>
    [DataField]
    public SoundSpecifier BootUpSound = new SoundPathSpecifier("/Audio/_Goobstation/Ambience/ominous_pulse.ogg");

    public TimeSpan NextUpdate;

    [DataField]
    public TimeSpan Interval;

    [DataField, AutoNetworkedField]
    public Color LightColor = Color.Cyan;
}
