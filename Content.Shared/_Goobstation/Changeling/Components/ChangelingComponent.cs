using Content.Shared.Alert;
using Content.Shared.Humanoid;
using Content.Shared.StatusIcon;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Changeling.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class ChangelingComponent : Component
{
    #region Prototypes

    [DataField]
    public SoundSpecifier MeatSounds = new SoundCollectionSpecifier("gib");

    [DataField]
    public SoundSpecifier ShriekSound = new SoundPathSpecifier("/Audio/_Goobstation/Changeling/Effects/changeling_shriek.ogg");

    [DataField, AutoNetworkedField]
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

    /// <summary>
    ///     The status icon corresponding to the Changlings.
    /// </summary>

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public ProtoId<FactionIconPrototype> StatusIcon { get; set; } = "HivemindFaction";

    [DataField]
    public ProtoId<AlertPrototype> ChemicalsAlert = "ChangelingChemicals";

    [DataField]
    public ProtoId<AlertPrototype> BiomassAlert = "ChangelingChemicals";

    [DataField]
    public string ChangelingBloodPrototype = "BloodChangeling";
    #endregion

    // TODO: MOVE THIS TO FAST RUN ABILITY
    public bool StrainedMusclesActive = false;

    public ChangelingFormType FormType = ChangelingFormType.HumanoidForm;

    public Dictionary<string, EntityUid?> Equipment = new();

    #region Biomass and Chemicals

    /// <summary>
    ///     Amount of biomass changeling currently has.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Biomass = 60f;

    /// <summary>
    ///     Maximum amount of biomass a changeling can have.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MaxBiomass = 30f;

    /// <summary>
    ///     How much biomass should be removed per cycle.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float BiomassDrain = 1f;

    /// <summary>
    ///     Current amount of chemicals changeling currently has.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Chemicals = 100f;

    /// <summary>
    ///     Maximum amount of chemicals changeling can have.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MaxChemicals = 100f;

    /// <summary>
    ///     Bonus chemicals regeneration. In case
    /// </summary>
    [DataField, AutoNetworkedField]
    public float BonusChemicalRegen = 0f;

    /// <summary>
    ///     Maximum bonus that ling can get from having 0 biomass
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MaxBonusChemicalRegen = 3f;

    /// <summary>
    ///     Turns off biomass gain until ling get new one. Just for optimization.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsEmptyBiomass = false;

    /// <summary>
    ///     Cooldown between chem regen events.
    /// </summary>
    public TimeSpan UpdateTimer = TimeSpan.Zero;
    public float UpdateCooldown = 1f;

    public float BiomassUpdateTimer = 0f;
    public float BiomassUpdateCooldown = 60f;

    #endregion

    [ViewVariables(VVAccess.ReadOnly)]
    public List<TransformData> AbsorbedDNA = new();

    /// <summary>
    ///     Index of <see cref="AbsorbedDNA"/>. Used for switching forms.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public int AbsorbedDNAIndex = 0;

    /// <summary>
    ///     Maximum amount of DNA a changeling can absorb.
    /// </summary>
    public int MaxAbsorbedDNA = 5;

    /// <summary>
    ///     Total absorbed DNA. Counts towards objectives.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public int TotalAbsorbedEntities = 0;

    /// <summary>
    ///     Total stolen DNA. Counts towards objectives.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public int TotalStolenDNA = 0;

    [ViewVariables(VVAccess.ReadOnly)]
    public TransformData? CurrentForm;

    [ViewVariables(VVAccess.ReadOnly)]
    public TransformData? SelectedForm;
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
}
