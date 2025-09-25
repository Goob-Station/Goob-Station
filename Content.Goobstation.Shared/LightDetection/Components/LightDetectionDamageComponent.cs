using Content.Shared.Alert;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.LightDetection.Components;

/// <summary>
/// Component that indicates a user should take damage or heal damage based on the light detection system
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState(false, true), AutoGenerateComponentPause]
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
    [ViewVariables, AutoNetworkedField]
    public float DetectionValue;

    /// <summary>
    /// How much detection entity regenerate in darkness.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float DetectionValueRegeneration = 0.5f;

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
            ["Heat"] = 10,
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
            ["Blunt"] = -10,
            ["Slash"] = -10,
            ["Piercing"] = -10,
            ["Heat"] = -10,
            ["Cold"] = -10,
            ["Shock"] = -10,
            ["Asphyxiation"] = -10,
            ["Bloodloss"] = -10,
            ["Poison"] = -10,
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

    [DataField(customTypeSerializer:typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan NextUpdate;

    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(1f);
}
