using Robust.Shared.Audio;


namespace Content.Shared._EE.Shadowling;


/// <summary>
/// This is used for the Nox Imperii ability.
/// </summary>
[RegisterComponent]
public sealed partial class ShadowlingNoxImperiiComponent : Component
{
    public string? ActionNoxImperii = "ActionNoxImperii";

    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(15);

    [DataField]
    public float Radius = 1.5f;

    [DataField]
    public float LightEnergy = 5f;
}
