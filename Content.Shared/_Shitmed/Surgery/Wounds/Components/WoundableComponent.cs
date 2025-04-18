using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class WoundableComponent : Component
{
    /// <summary>
    /// UID of the parent woundable entity. Can be null.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public EntityUid? ParentWoundable;

    /// <summary>
    /// UID of the root woundable entity.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public EntityUid RootWoundable;

    /// <summary>
    /// Set of UIDs representing child woundable entities.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public HashSet<EntityUid> ChildWoundables = [];

    /// <summary>
    /// Indicates whether wounds are allowed.
    /// </summary>
    [DataField]
    [ViewVariables, AutoNetworkedField]
    public bool AllowWounds = true;

    /// <summary>
    /// The same as DamageableComponent's one
    /// </summary>
    [DataField("damageContainer")]
    public ProtoId<DamageContainerPrototype>? DamageContainerID;

    /// <summary>
    /// Integrity points of this woundable.
    /// </summary>
    [DataField]
    [ViewVariables, AutoNetworkedField]
    public FixedPoint2 IntegrityCap;

    /// <summary>
    /// How big is the Woundable Entity, mostly used for trauma calculation, dodging and targeting
    /// </summary>
    [DataField]
    [ViewVariables, AutoNetworkedField]
    public FixedPoint2 DodgeChance = 0.1;

    /// <summary>
    /// Integrity points of this woundable.
    /// </summary>
    [DataField("integrity")]
    [ViewVariables, AutoNetworkedField]
    public FixedPoint2 WoundableIntegrity;

    /// <summary>
    /// yeah
    /// </summary>
    [DataField(required: true)]
    public Dictionary<WoundableSeverity, FixedPoint2> Thresholds = new();

    /// <summary>
    /// How much damage will be healed ACROSS all limb, for example if there are 2 wounds,
    /// Healing will be shared across those 2 wounds.
    /// </summary>
    [DataField]
    [ViewVariables, AutoNetworkedField]
    public FixedPoint2 HealAbility = 0.1;

    /// <summary>
    /// Multipliers of severity applied to this wound.
    /// </summary>
    public Dictionary<EntityUid, WoundableSeverityMultiplier> SeverityMultipliers = new();

    /// <summary>
    /// Multipliers applied to healing rate.
    /// </summary>
    public Dictionary<EntityUid, WoundableHealingMultiplier> HealingMultipliers = new();

    [DataField]
    public SoundSpecifier WoundableDestroyedSound = new SoundCollectionSpecifier("WoundableDestroyed");

    [DataField]
    public SoundSpecifier WoundableDelimbedSound = new SoundCollectionSpecifier("WoundableDelimbed");

    /// <summary>
    /// State of the woundable. Severity basically.
    /// </summary>
    [DataField]
    [ViewVariables, AutoNetworkedField]
    public WoundableSeverity WoundableSeverity;

    /// <summary>
    /// How much time in seconds had this woundable accumulated from the last healing tick.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public float HealingRateAccumulated;

    /// <summary>
    /// Container potentially holding wounds.
    /// </summary>
    [ViewVariables]
    public Container Wounds = new();

    /// <summary>
    /// Container holding this woundables bone.
    /// </summary>
    [ViewVariables]
    public Container Bone = new();

    /// <summary>
    /// Whether this woundable can be removed from a body..
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public bool CanRemove = true;

    /// <summary>
    /// Whether this woundable's bone is exposed
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public bool IsBoneExposed = false;


}
