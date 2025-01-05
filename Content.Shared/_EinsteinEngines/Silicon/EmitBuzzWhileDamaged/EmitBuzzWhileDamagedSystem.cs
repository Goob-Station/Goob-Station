using Robust.Shared.Audio;

namespace Content.Shared._EinsteinEngines.Silicon.EmitBuzzWhileDamaged;

/// <summary>
/// This is used for controlling the cadence of the buzzing emitted by EmitBuzzOnCritSystem.
/// This component is used by mechanical species that can get to critical health.
/// </summary>
[RegisterComponent]
public sealed partial class EmitBuzzWhileDamagedComponent : Component
{
    [DataField("buzzPopupCooldown")]
    public TimeSpan BuzzPopupCooldown { get; private set; } = TimeSpan.FromSeconds(8);

    [ViewVariables]
    public TimeSpan LastBuzzPopupTime;

    [DataField("cycleDelay")]
    public float CycleDelay = 2.0f;

    public float AccumulatedFrametime;

    [DataField("sound")]
    public SoundSpecifier Sound = new SoundCollectionSpecifier("buzzes");
}
