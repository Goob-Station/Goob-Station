using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Hamon.Components;

/// <summary>
/// Grants all the hamon components when this component is added to someone also handles hamon infusing.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HamonUserComponent : Component
{
    [DataField(required: true)]
    public ComponentRegistry Components;

    [DataField]
    public List<EntProtoId> Actions;

    [DataField]
    public EntProtoId AssPullEntity = "WeaponSubMachineGunC20r";

    [DataField]
    public float InfuseBonusTime = 1f;

    [DataField]
    public SoundSpecifier OverdriveSound = new SoundPathSpecifier("/Audio/_Goobstation/Weapons/Melee/hamon.ogg");

    [DataField]
    public bool OverdriveActive;

    [DataField]
    public DamageSpecifier OverdriveBonusDamage = new()
    {
        DamageDict = new()
        {
            { "Holy", 200},
            { "Blunt", 3},
            { "Shock", 50},
        }
    };
}
