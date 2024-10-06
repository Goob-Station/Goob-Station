using Content.Shared._Goobstation.Changeling.EntitySystems;
using Content.Shared.Alert;
using Content.Shared.StatusIcon;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Goobstation.Changeling.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class ChangelingComponent : Component
{
    #region Prototypes

    [DataField]
    public SoundSpecifier MeatSounds = new SoundCollectionSpecifier("gib");

    [DataField]
    public SoundSpecifier ShriekSound = new SoundPathSpecifier("/Audio/_Goobstation/Changeling/Effects/changeling_shriek.ogg");

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

    // TODO: Replace this with Container
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

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan LastChemicalsUpdate = TimeSpan.Zero;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan LastBiomassUpdate = TimeSpan.Zero;

    [DataField]
    public TimeSpan ChemicalsUpdateCooldown = TimeSpan.FromSeconds(1);

    [DataField]
    public TimeSpan BiomassUpdateCooldown = TimeSpan.FromMinutes(1);

    #endregion

    #region Ling forms
    [DataField, AutoNetworkedField]
    public ChangelingFormType FormType = ChangelingFormType.HumanoidForm;

    [ViewVariables(VVAccess.ReadOnly)]
    public TransformData? CurrentForm;

    [ViewVariables(VVAccess.ReadOnly)]
    public TransformData? SelectedForm;
    #endregion

    #region DNA

    [DataField, AutoNetworkedField]
    public List<TransformData> AbsorbedDNA = new();

    // Delete this - replace with radial ui
    /// <summary>
    ///     Index of <see cref="AbsorbedDNA"/>. Used for switching forms.
    /// </summary>
    [DataField]
    public int AbsorbedDNAIndex = 0;

    /// <summary>
    ///     Maximum amount of DNA a changeling can absorb.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int MaxAbsorbedDNA = 5;

    /// <summary>
    ///     Total absorbed DNA. Counts towards objectives.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int TotalAbsorbedEntities = 0;

    /// <summary>
    ///     Total stolen DNA. Counts towards objectives.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int TotalStolenDNA = 0;

    #endregion
}

[DataDefinition, Serializable, NetSerializable]
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
    ///     Entity humanoid that contain data appearance.
    /// </summary>
    [DataField]
    public NetEntity AppearanceEntity;
}
