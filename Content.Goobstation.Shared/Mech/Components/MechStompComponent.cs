using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Mech.Components;

/// <summary>
/// Gives a short stun to everyone nearby.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MechStompComponent : Component
{
    [DataField]
    public EntProtoId ActionProto = "ActionMechStomp";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;

    [DataField]
    public TimeSpan StompWindUpDoAfter = TimeSpan.FromSeconds(1.5);

    [DataField]
    public float StompRadius = 1.6f;

    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new() { { "Blunt", 10 } },
    };

    [DataField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(3);

    [DataField]
    public float KnockbackSpeed = 5f;

    [DataField]
    public FixedPoint2 PowerCageEnergyCost = 40;

    [DataField]
    public float CameraKick = 2.5f;

    [DataField]
    public SoundSpecifier WindUpSound = new SoundPathSpecifier("/Audio/Mecha/sound_mecha_hydraulic.ogg", AudioParams.Default.WithVolume(-9f));
}
