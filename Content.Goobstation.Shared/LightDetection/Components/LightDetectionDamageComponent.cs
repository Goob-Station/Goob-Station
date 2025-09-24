using Content.Shared.Alert;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.LightDetection.Components;

/// <summary>
/// Component that indicates a user should take damage or heal damage based on the light detection system
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState(false, true)]
public sealed partial class LightDetectionDamageComponent : Component
{
    /// <summary>
    /// Max Detection Value
    /// </summary>
    [DataField("maxDetection"), AutoNetworkedField]
    public float DetectionValueMax = 5f;

    /// <summary>
    /// If this reaches 0, the entity will start taking damage.
    /// If it is max, the entity will heal damage (if specified)
    /// </summary>
    [DataField, AutoNetworkedField]
    public float DetectionValue;

    [DataField, AutoNetworkedField]
    public float DetectionValueFactor = 0.5f;

    /// <summary>
    /// Indicates whether the user should take damage on light
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool TakeDamageOnLight = true;

    /// <summary>
    /// Indicates whether the user should heal if not on light
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool HealOnShadows = true;

    /// <summary>
    ///  For shadowlings (Light Resistance)
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ResistanceModifier = 1;

    /// <summary>
    /// How much damage to deal to the entity.
    /// Shadowlings will have Light Resistance, so this will get affected by that.
    /// </summary>
    [DataField]
    public DamageSpecifier DamageToDeal = new()
    {
        DamageDict = new()
        {
            ["Heat"] = 15,
        }
    };

    /// <summary>
    /// How much damage to heal to the entity.
    /// </summary>
    [DataField]
    public DamageSpecifier DamageToHeal = new()
    {
        DamageDict = new()
        {
            ["Blunt"] = -15,
            ["Slash"] = -15,
            ["Piercing"] = -15,
            ["Heat"] = -15,
            ["Cold"] = -15,
            ["Shock"] = -15,
            ["Asphyxiation"] = -15,
            ["Bloodloss"] = -15,
            ["Poison"] = -15,
        }
    };

    [DataField]
    public ProtoId<AlertPrototype> AlertProto = "ShadowlingLight";

    [DataField]
    public int AlertMaxSeverity = 9;

    [DataField]
    public SoundSpecifier? SoundOnDamage = new SoundPathSpecifier("/Audio/Weapons/Guns/Hits/energy_meat1.ogg");

    /// <summary>
    /// If an alert prototype does not exist, this should be false. Otherwise, it is defaulted to the Shadowling's one.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ShowAlert = true;

    [DataField]
    public float Accumulator;

    [DataField]
    public float UpdateInterval = 1f;

    [DataField]
    public float DamageInterval = 5f;

    [DataField]
    public float DamageAccumulator;

    [DataField]
    public float HealInterval = 3f;

    [DataField]
    public float HealAccumulator;
}
