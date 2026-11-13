using Content.Shared.Damage;
using Content.Shared.Dataset;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Flagpole;

/// <summary>
/// Deals damage or heals in a range, affecting only mindshielded personnel.
/// Also can enhance melee damage against mindshielded targets.
/// Also has an active use that heals mindshielded personnel in critical state nearby and makes the user say a line.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class FlagpoleComponent : Component
{
    /// <summary>
    /// Radius of effect if you're actualyl using one. 0 just makes it not have any.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Range;

    /// <summary>
    /// How often the radius effect is applied.
    /// </summary>
    [DataField]
    public TimeSpan Interval = TimeSpan.FromSeconds(1);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan NextUpdate;

    /// <summary>
    /// How often expired stacking claims are cleaned up.
    /// </summary>
    [DataField]
    public TimeSpan PruneInterval = TimeSpan.FromSeconds(30);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextPrune;

    /// <summary>
    /// Damage (or healing if you use negative values) done to folks in the range every time interval runs.
    /// </summary>
    [DataField]
    public DamageSpecifier Damage = new();

    /// <summary>
    /// Status effect proto that can be applied if you want.
    /// </summary>
    [DataField]
    public Dictionary<EntProtoId, TimeSpan> StatusEffects = new();

    /// <summary>
    /// Flat extra damage to deal to mindshielded targets if this is on a weapon.
    /// </summary>
    [DataField]
    public DamageSpecifier MeleeDamage = new();

    /// <summary>
    /// If the effect of this thing can stack with other items of the same type. For AoE healing/damaging you probably really
    /// dont want this to stack because powergamers will powergame.
    /// </summary>
    [DataField]
    public bool AllowMultipleSources;

    /// <summary>
    /// Only stuff of the same group prevents effects from stacking
    /// </summary>
    [DataField]
    public string StackGroup = string.Empty;

    /// <summary>
    /// i wonder
    /// </summary>
    [DataField]
    public bool IgnoreResistances;

    /// <summary>
    /// Radius of the activated effect. 0 disables it.
    /// </summary>
    [DataField]
    public float UseRange;

    /// <summary>
    /// Applies healing or damage depending on value set to nearby mindshielded personnel. Negative numbers will heal while positive numbers will deal damage.
    /// </summary>
    [DataField]
    public DamageSpecifier ApplyOnCrit = new();

    /// <summary>
    /// Dataset of lines the user says when activating. One is picked at random.
    /// </summary>
    [DataField]
    public ProtoId<LocalizedDatasetPrototype>? UseSpeech;
}
