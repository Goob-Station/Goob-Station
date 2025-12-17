using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.Polymorph;
using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Shadowling.Components;

// <summary>
// Handles the main actions of a Shadowling
// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class ShadowlingComponent : Component
{
    [DataField]
    public EntProtoId ActionHatch = "ActionHatch";

    [DataField]
    public EntityUid? ActionHatchEntity;

    /// <summary>
    /// The status icon for Shadowlings
    /// </summary>
    [DataField]
    public ProtoId<FactionIconPrototype> StatusIcon { get; set; } = "ShadowlingFaction";

    /// <summary>
    /// Phase Indicator. The Shadowlings have 4 phases currently, as seen at the bottom of this component.
    /// The first phase is the pre-hatch, where Shadowlings start with only their basic hatch ability and are disguised as their normal character.
    /// The second phase, "post-hatch", where Shadowlings transform into a seperate species with new abilities.
    /// The third phase, "ascension", where Shadowlings transform into the ascendant with new abilities, while having their previous removed.
    /// A failed phase, "failed ascension", where Shadowlings turn are slowed down and lose all their abilities. Only used for Ascension Egg.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ShadowlingPhases CurrentPhase = ShadowlingPhases.PreHatch;

    /// <summary>
    /// Indicates whether the shadowling is hatching, or not.
    /// </summary>
    [DataField]
    public bool IsHatching;

    /// <summary>
    /// The species of the Shadowling
    /// </summary>
    [DataField]
    public ProtoId<PolymorphPrototype> ShadowlingPolymorphId = "ShadowlingPolymorph";

    /// <summary>
    /// The first ability's egg prototype
    /// </summary>
    [DataField]
    public EntProtoId Egg = "SlingEggHatch";

    /// <summary>
    /// Abilities components that will be added on hatch and removed on ascension.
    /// </summary>
    [DataField]
    public EntProtoId PostHatchComponents = "ShadowlingAbilitiesPostHatch";

    /// <summary>
    /// All components that can be obtained by converting more thralls
    /// and using Collective mind ability.
    /// </summary>
    [DataField]
    public EntProtoId ObtainableComponents = "ShadowlingAbilitiesObtainable";

    /// <summary>
    /// Abilities components that will be added on ascension.
    /// </summary>
    [DataField]
    public EntProtoId PostAscensionComponents = "ShadowlingAbilitiesAscension";

    /// <summary>
    /// Thralls of the shadowling.
    /// </summary>
    [DataField]
    public HashSet<EntityUid> Thralls = new();

    /// <summary>
    /// This is used for the resistance that the Shadowling gets against damage from lights only.
    /// It is a core mechanic for balance.
    /// It can be decreased once you Thrall someone, or once you use Black Recuperation (the values are changed there, however).
    /// Once a Thrall gets removed from the Shadowling's Thralls, this gets decreased from the overall resistance.
    /// The overall resistance exists in the system LightDetectionDamageModifier, which (for this case) handles taking damage while standing on light.
    /// Gets removed once Nox Imperii ability is used.
    /// </summary>
    [DataField]
    public float LightResistanceModifier = 0.12f;

    /// <summary>
    /// Indicates how much damage the Shadowling takes from Heat sources like flashbangs.
    /// </summary>
    [DataField]
    public DamageSpecifier HeatDamage = new()
    {
        DamageDict = new()
        {
            ["Heat"] = 20,
        }
    };

    /// <summary>
    /// Same as above, except this is used for laser weapons
    /// </summary>
    [DataField]
    public DamageSpecifier HeatDamageProjectileModifier = new()
    {
        DamageDict = new()
        {
            ["Heat"] = 10,
        }
    };

    /// <summary>
    /// This is the damage the Shadowlings take once they try to shoot a gun. It is low, because it should just be an indication that
    /// the Shadowling can't shoot guns, rather than hurting the Shadowling itself.
    /// </summary>
    [DataField]
    public DamageSpecifier GunShootFailDamage = new()
    {
        DamageDict = new()
        {
            ["Blunt"] = 5,
            ["Piercing"] = 4,
        }
    };

    /// <summary>
    /// Used for long the Shadowlings get stunned once they fail to shoot a gun.
    /// </summary>
    [DataField]
    public TimeSpan GunShootFailStunTime = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Used to indicate whether the Shadowling is currently ascending.
    /// This is used by the Ascension Egg system to prevent 2 or more shadowlings ascending at the same time.
    /// </summary>
    [DataField]
    public bool IsAscending;

    /// <summary>
    /// The objective prototype that the Shadowling has.
    /// </summary>
    [DataField]
    public EntProtoId ObjectiveAscend = "ShadowlingAscendObjective";
}

[NetSerializable, Serializable]
public enum ShadowlingPhases : byte
{
    PreHatch,
    PostHatch,
    Ascension,
    FailedAscension,
}
