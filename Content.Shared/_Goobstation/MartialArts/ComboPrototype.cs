using Content.Shared._Goobstation.MartialArts.Components;
using Content.Shared._Goobstation.MartialArts.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.MartialArts;

[Prototype("combo")]
[Serializable, NetSerializable, DataDefinition]
public sealed partial class ComboPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public MartialArtsForms MartialArtsForm;

    [DataField("attacks", required: true)]
    public List<ComboAttackType> AttackTypes = new();

    //[DataField("weapon")] // Will be done later
    //public string? WeaponProtoId;
    [DataField("event", required: true)]
    public object? ResultEvent;

    /// <summary>
    /// How much extra damage should this move do on perform?
    /// </summary>
    [DataField]
    public int ExtraDamage;

    /// <summary>
    /// Stun time in seconds
    /// </summary>
    [DataField]
    public int ParalyzeTime;

    /// <summary>
    /// How much stamina damage should this move do on perform.
    /// </summary>
    [DataField]
    public float StaminaDamage;

    /// <summary>
    /// Blunt, Slash, etc.
    /// </summary>
    [DataField]
    public string DamageType = "Blunt";

    /// <summary>
    /// How fast people are thrown on combo
    /// </summary>
    [DataField]
    public float ThrownSpeed = 7f;

    /// <summary>
    /// Name of the move
    /// </summary>
    [DataField(required: true)]
    public string Name = string.Empty;

}

[Prototype("comboList")]
public sealed partial class ComboListPrototype : IPrototype
{
    [IdDataField] public string ID { get; private init; } = default!;

    [DataField( required: true)]
    public List<ProtoId<ComboPrototype>> Combos = new();
}
