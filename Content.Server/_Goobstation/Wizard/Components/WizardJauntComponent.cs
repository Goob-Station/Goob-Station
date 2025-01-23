using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Wizard.Components;

[RegisterComponent]
public sealed partial class WizardJauntComponent : Component
{
    [DataField]
    public EntProtoId JauntStartEffect = "EtherealJauntStartEffect";

    [DataField]
    public EntProtoId JauntEndEffect = "EtherealJauntEndEffect";

    [DataField]
    public SoundSpecifier JauntStartSound = new SoundPathSpecifier("/Audio/Magic/ethereal_enter.ogg");

    [DataField]
    public SoundSpecifier JauntEndSound = new SoundPathSpecifier("/Audio/Magic/ethereal_exit.ogg");

    [DataField]
    public float DurationBetweenEffects = 2.8f;

    [DataField]
    public bool JauntEndEffectSpawned;
}
