using Content.Shared.Humanoid;
using Content.Shared.StatusIcon;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Changeling;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class ChangelingComponent : Component
{
    [DataField("soundMeatPool")]
    public List<SoundSpecifier?> SoundPool = new()
    {
        new SoundPathSpecifier("/Audio/Effects/gib1.ogg"),
        new SoundPathSpecifier("/Audio/Effects/gib2.ogg"),
        new SoundPathSpecifier("/Audio/Effects/gib3.ogg"),
    };

    #region Abilities

    [DataField("soundShriek")]
    public SoundSpecifier ShriekSound = new SoundPathSpecifier("/Audio/Goobstation/Changeling/Effects/changeling_shriek.ogg");

    [DataField("shriekPower")]
    public float ShriekPower = 2.5f;

    public readonly List<ProtoId<EntityPrototype>> BaseChangelingActions = new()
    {
        "ActionEvolutionMenu",
        "ActionAbsorbDNA",
        "ActionStingExtractDNA",
        "ActionChangelingTransformCycle",
        "ActionChangelingTransform",
        "ActionEnterStasis",
        "ActionExitStasis"
    };

    public bool IsInStasis = false;

    public EntityUid? ArmbladeEntity;
    public EntityUid? ShieldEntity;
    public EntityUid? ArmorEntity, ArmorHelmetEntity;
    public EntityUid? SpacesuitEntity, SpacesuitHelmetEntity;

    public bool StrainedMusclesActivated = false;

    public bool IsInLesserForm = false;

    #endregion

    #region Base

    /// <summary>
    ///     Current amount of chemicals changeling currently has.
    /// </summary>
    [DataField("chemicals"), AutoNetworkedField]
    public float Chemicals = 100f;

    /// <summary>
    ///     Maximum amount of chemicals changeling can have.
    /// </summary>
    [DataField("maxChemicals"), AutoNetworkedField]
    public float MaxChemicals = 100f;

    public float ChemicalDrain = 0f;

    public float UpdateAccumulator = 0f;
    /// <summary>
    ///     Time in seconds to take before the update cycle.
    /// </summary>
    public readonly float UpdateTimer = 1f;


    [ViewVariables(VVAccess.ReadOnly)]
    public List<TransformData> AbsorbedDNA = new();
    /// <summary>
    ///     Index of <see cref="AbsorbedDNA"/>. Used for switching forms.
    /// </summary>
    public int AbsorbedDNAIndex = 0;

    /// <summary>
    ///     Maximum amount of DNA a changeling can absorb.
    /// </summary>
    [DataField("maxDna")]
    public int MaxAbsorbedDNA = 5;

    /// <summary>
    ///     Total absorbed DNA. Counts towards objectives.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public int TotalAbsorbedEntities = 0;

    /// <summary>
    ///     Total stolen DNA. Counts towards objectives.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public int TotalStolenDNA = 0;

    [ViewVariables(VVAccess.ReadOnly)]
    public TransformData? CurrentForm;

    [ViewVariables(VVAccess.ReadOnly)]
    public TransformData? SelectedForm;

    #endregion
    /// <summary>
    /// The status icon corresponding to the Changlings.
    /// </summary>

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<StatusIconPrototype> StatusIcon { get; set; } = "HivemindFaction";
}

[DataDefinition]
public sealed partial class TransformData
{
    /// <summary>
    ///     Entity's name.
    /// </summary>
    [DataField]
    public string Name;

    /// <summary>
    ///     Entity's fingerprint, if it exists.
    /// </summary>
    [DataField]
    public string? Fingerprint;

    /// <summary>
    ///     Entity's DNA.
    /// </summary>
    [DataField("dna")]
    public string DNA;

    /// <summary>
    ///     Entity's humanoid appearance component.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), NonSerialized]
    public HumanoidAppearanceComponent Appearance;

    //public static bool operator ==(TransformData one, TransformData two)
    //    => one.Name == two.Name && one.Fingerprint == two.Fingerprint && one.DNA == two.DNA;
    //public static bool operator !=(TransformData one, TransformData two)
    //    => !(one.Name == two.Name && one.Fingerprint == two.Fingerprint && one.DNA == two.DNA);
}
