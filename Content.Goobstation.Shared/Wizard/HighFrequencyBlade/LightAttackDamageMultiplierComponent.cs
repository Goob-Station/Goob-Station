using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wizard.HighFrequencyBlade;

[RegisterComponent, NetworkedComponent]
public sealed partial class LightAttackDamageMultiplierComponent : Component
{
    [DataField]
    public float Multiplier = 2f;

    [DataField]
    public SoundSpecifier? ExtraSound;
}
