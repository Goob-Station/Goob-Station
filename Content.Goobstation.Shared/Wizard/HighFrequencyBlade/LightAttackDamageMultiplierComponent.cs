using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Wizard.HighFrequencyBlade;

[RegisterComponent, NetworkedComponent]
public sealed partial class LightAttackDamageMultiplierComponent : Component
{
    [DataField]
    public float Multiplier = 2f;

    [DataField]
    public SoundSpecifier? ExtraSound;
}
