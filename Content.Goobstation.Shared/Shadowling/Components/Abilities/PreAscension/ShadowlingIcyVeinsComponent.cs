using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Components.Abilities.PreAscension;

/// <summary>
/// This is used for Icy Veins.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingIcyVeinsComponent : Component
{
    [DataField]
    public EntProtoId ActionIcyVeins = "ActionIcyVeins";

    [DataField]
    public float Range = 6f;

    [DataField]
    public float ParalyzeTime = 1f;

    [DataField]
    public EntProtoId IcyVeinsEffect = "ShadowlingIcyVeinsEffect";

    [DataField]
    public SoundSpecifier? IcyVeinsSound = new SoundPathSpecifier("/Audio/Effects/ghost2.ogg");
}
