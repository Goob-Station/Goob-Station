using Content.Shared.Damage;

namespace Content.Goobstation.Server.Medical.ApplyPressure;

[RegisterComponent]
public sealed partial class CanApplyPressureComponent : Component
{
    /// <summary>
    /// How long each pressure application takes to finish.
    /// </summary>
    [DataField]
    public TimeSpan DoAfterDuration = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The amount bleed is modified by per pressure application.
    /// </summary>
    [DataField]
    public float BleedModifier;
}
