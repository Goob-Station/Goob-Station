using Content.Shared.Humanoid;
using Content.Shared.StatusIcon;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Changeling.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class ChangelingComponent : Component
{
    /// <summary>
    ///     Sound pool used for audible abilities.
    /// </summary>
    [DataField] public List<SoundSpecifier?> AbilitySoundPool = new()
    {
        new SoundPathSpecifier("/Audio/Effects/gib1.ogg"),
        new SoundPathSpecifier("/Audio/Effects/gib2.ogg"),
        new SoundPathSpecifier("/Audio/Effects/gib3.ogg"),
    };

    /// <summary>
    ///     Contains all changeling exclusive equipment for further processing.
    /// </summary>
    [DataField] public Dictionary<EntProtoId, ChangelingEquipmentData?> Equipment = new();

    public bool IsInStasis = false;

    public bool IsInLesserForm = false;

    [DataField] public SoundSpecifier ShriekSound = new SoundPathSpecifier("/Audio/Goobstation/Changeling/Effects/changeling_shriek.ogg");

    [DataField] public float ShriekPower = 2.5f;

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

    public bool StrainedMusclesActive = false;

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

    [ViewVariables(VVAccess.ReadOnly)]
    public TransformData? CurrentForm;

    [ViewVariables(VVAccess.ReadOnly)]
    public TransformData? SelectedForm;

    /// <summary>
    ///     Cooldown between chem regen events.
    /// </summary>
    public TimeSpan RegenTime = TimeSpan.Zero;
    public float RegenCooldown = 1f;

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
    [DataField] public int MaxAbsorbedDNA = 5;

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
}

[DataDefinition]
public sealed partial class TransformData
{
    /// <summary>
    ///     Entity's name.
    /// </summary>
    [DataField] public string Name;

    /// <summary>
    ///     Entity's fingerprint, if it exists.
    /// </summary>
    [DataField] public string? Fingerprint;

    /// <summary>
    ///     Entity's DNA.
    /// </summary>
    [DataField] public string DNA;

    /// <summary>
    ///     Entity's humanoid appearance component.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), NonSerialized]
    public HumanoidAppearanceComponent Appearance;
}

[DataDefinition]
public sealed partial class ChangelingEquipmentData
{
    [DataField] public EntProtoId Prototype;
    [DataField] public string? EquipmentSlot;
    public EntityUid? Entity;

    public ChangelingEquipmentData(EntityUid? ent, string? slot) : base()
    {
        Entity = ent;
        EquipmentSlot = slot;
    }
}
