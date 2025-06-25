using Content.Shared.Atmos;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;


namespace Content.Shared._EE.Shadowling;


/// <summary>
/// This is used for Icy Veins.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingIcyVeinsComponent : Component
{
    public string? ActionIcyVeins = "ActionIcyVeins";

    [DataField]
    public float Range = 6f;

    [DataField]
    public float ParalyzeTime = 1f;

    [DataField]
    public string? IcyVeinsEffect = "ShadowlingIcyVeinsEffect";

    [DataField]
    public SoundSpecifier? IcyVeinsSound = new SoundPathSpecifier("/Audio/Effects/ghost2.ogg");
}
