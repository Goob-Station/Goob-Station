using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Wizard.SanguineStrike;

[RegisterComponent, NetworkedComponent]
public sealed partial class SanguineStrikeComponent : Component
{
    [DataField]
    public float Lifetime = 15f;

    [DataField]
    public float DamageMultiplier = 2f;

    [DataField]
    public float MaxDamageModifier = 20f;

    [DataField]
    public EntProtoId Effect = "SanguineFlashEffect";

    [DataField]
    public Color Color = Color.FromHex("#C41515");

    [DataField]
    public float LightRadius = 4f;

    [DataField]
    public float LightEnergy = 3f;

    [DataField]
    public FixedPoint2 BloodSuckAmount = 50;

    [DataField]
    public EntProtoId BloodEffect = "SanguineBloodEffect";

    [DataField]
    public SoundSpecifier HitSound = new SoundPathSpecifier("/Audio/_Goobstation/Wizard/crackandbleed.ogg");

    [DataField]
    public SoundSpecifier LifestealSound = new SoundPathSpecifier("/Audio/_Goobstation/Wizard/charge.ogg");

    [ViewVariables(VVAccess.ReadOnly)]
    public bool HadPointLight;

    [ViewVariables(VVAccess.ReadOnly)]
    public Color OldColor = Color.White;
}
