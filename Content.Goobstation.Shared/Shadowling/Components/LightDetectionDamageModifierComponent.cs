using Content.Shared.Alert;
using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;


namespace Content.Shared._EE.Shadowling.Components;


/// <summary>
/// Component that indicates a user should take damage or heal damage based on the light detection system
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class LightDetectionDamageModifierComponent : Component
{
    /// <summary>
    /// Max Detection Value
    /// </summary>
    [DataField, AutoNetworkedField]
    public float DetectionValueMax = 5f;

    /// <summary>
    /// If this reaches 0, the entity will start taking damage.
    /// If it is max, the entity will heal damage (if specified)
    /// </summary>
    [DataField, AutoNetworkedField]
    public float DetectionValue;

    /// <summary>
    /// Indicates whether the user should take damage on light
    /// </summary>
    [DataField]
    public bool TakeDamageOnLight = true;

    /// <summary>
    /// Indicates whether the user should heal if not on light
    /// </summary>
    [DataField]
    public bool HealOnShadows = true;

    [DataField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    /// <summary>
    /// How often to decrease the DetectionValue.
    ///
    /// Example:
    /// If a shadowling is standing on light, and this is 1f, and DetectionTimerDecreaseFactor is 1f then
    /// it will take 5 seconds (considering DetectionValue is 5f) for the entity to start taking damage.
    /// </summary>
    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(0.5f);

    [DataField]
    public float DetectionValueFactor = 0.5f;

    /// <summary>
    /// How often the entity will take damage
    /// </summary>
    [DataField]
    public TimeSpan DamageInterval = TimeSpan.FromSeconds(5f);

    [DataField]
    public TimeSpan NextUpdateDamage = TimeSpan.Zero;

    /// <summary>
    /// How often the entity will heal damage
    /// </summary>
    [DataField]
    public TimeSpan HealInterval = TimeSpan.FromSeconds(3f);

    [DataField]
    public TimeSpan NextUpdateHeal = TimeSpan.Zero;

    /// <summary>
    ///  For shadowlings (Light Resistance)
    /// </summary>
    [DataField]
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
    public bool ShowAlert = true;
}
