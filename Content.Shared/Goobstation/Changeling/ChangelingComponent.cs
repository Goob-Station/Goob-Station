using Content.Shared.Humanoid;
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


    public ProtoId<EntityPrototype> ArmbladePrototype = "ArmBladeChangeling";
    public EntityUid? ArmbladeEntity;

    public ProtoId<EntityPrototype> BoneShardPrototype = "ThrowingStarChangeling";

    public ProtoId<EntityPrototype> ArmorPrototype = "ChangelingClothingOuterArmor";
    public ProtoId<EntityPrototype> ArmorHelmetPrototype = "ChangelingClothingHeadHelmet";

    public ProtoId<EntityPrototype> SpacesuitPrototype = "ChangelingClothingOuterHardsuit";
    public ProtoId<EntityPrototype> SpacesuitHelmetPrototype = "ChangelingClothingHeadHelmetHardsuit";

    /// <summary>
    ///     Current amount of chemicals changeling currently has.
    /// </summary>
    [DataField("chemicals"), AutoNetworkedField]
    public float Chemicals = 100;

    /// <summary>
    ///     Maximum amount of chemicals changeling can have.
    /// </summary>
    [DataField("maxChemicals")]
    public float MaxChemicals = 100;

    public float ChemicalRegenerationAccumulator = 0;
    /// <summary>
    ///     Time in seconds to take before chemical regeneration occurs.
    /// </summary>
    public readonly float ChemicalRegenerationTimer = 1;

    public float ChemicalRegenerationMobStateModifier = 0;
    /// <summary>
    ///     Modifier for chemical regeneration. Positive = faster, negative = slower.
    /// </summary>
    public float ChemicalRegenerationModifier = 0;
    /// <summary>
    ///     The higher - the more chemicals drain.
    /// </summary>
    public float ChemicalDrain = 0;



    public List<TransformData> AbsorbedDNA = new();
    /// <summary>
    ///     Index of <see cref="AbsorbedDNA"/>. Used for switching forms.
    /// </summary>
    public int AbsorbedDNAIndex = 0;

    /// <summary>
    ///     Maximum amount of DNA a changeling can absorb.
    /// </summary>
    public int MaxAbsorbedDNA = 5;

    /// <summary>
    ///     Total absorbed DNA. Counts towards objectives.
    /// </summary>
    public int TotalAbsorbedEntities = 0;

    /// <summary>
    ///     Total stolen DNA. Counts towards objectives.
    /// </summary>
    public int TotalStolenDNA = 0;

    public TransformData? CurrentForm;

    public TransformData? SelectedForm;
}

public struct TransformData
{
    /// <summary>
    ///     Entity's name.
    /// </summary>
    public string Name;

    /// <summary>
    ///     Entity's fingerprint, if it exists.
    /// </summary>
    public string? Fingerprint;

    /// <summary>
    ///     Entity's DNA.
    /// </summary>
    public string DNA;

    /// <summary>
    ///     Entity's humanoid appearance component.
    /// </summary>
    public HumanoidAppearanceComponent Appearance;

    public static bool operator ==(TransformData one, TransformData two)
        => one.Name == two.Name && one.Fingerprint == two.Fingerprint && one.DNA == two.DNA;
    public static bool operator !=(TransformData one, TransformData two)
        => !(one.Name == two.Name && one.Fingerprint == two.Fingerprint && one.DNA == two.DNA);
}
