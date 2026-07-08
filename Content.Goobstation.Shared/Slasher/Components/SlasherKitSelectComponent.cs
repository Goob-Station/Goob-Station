using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Content.Shared.Dataset;
using Content.Shared.Roles;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Allows the Slasher to choose their kit (starting gear) when they first spawn.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SlasherKitSelectComponent : Component
{
    [DataField]
    public bool KitSelected;

    [DataField(required: true)]
    public Dictionary<string, SlasherKit> Kits = [];

    [DataField]
    public ComponentRegistry PostSelectionComponents = [];
}

[DataDefinition]
public sealed partial class SlasherKit
{
    /// <summary>
    /// Gear to equip on selection.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<StartingGearPrototype> Gear;

    /// <summary>
    /// Localization key for the kit description shown in the UI.
    /// </summary>
    [DataField]
    public string Description = string.Empty;

    /// <summary>
    /// Icon sprite shown in the kit selection UI.
    /// </summary>
    [DataField]
    public SpriteSpecifier Sprite = SpriteSpecifier.Invalid;

    /// <summary>
    /// Optional machete prototype override for machete summon on this kit.
    /// If null, the default machete on SlasherSummonMacheteComponent is kept.
    /// </summary>
    [DataField]
    public EntProtoId? MachetePrototype;

    /// <summary>
    /// Optional music override for the blood trail sound on this kit.
    /// If null, the default sound on SlasherFearComponent is kept.
    /// </summary>
    [DataField]
    public SoundSpecifier? BloodTrailMusic;

    /// <summary>
    /// Optional jumpscare sound override for the blood trail sound on this kit.
    /// If null, the default sounds on SlasherFearComponent are kept.
    /// </summary>
    [DataField]
    public SoundSpecifier? JumpscareSound;

    /// <summary>
    /// The fear-overlay style components this kit inflicts on victims.
    /// </summary>
    [DataField]
    public ComponentRegistry FearStyle = new();

    /// <summary>
    /// Optional meat spike prototype override for this kit.
    /// If null, the default SlasherSummonMeatSpikeComponent prototype is kept.
    /// </summary>
    [DataField]
    public EntProtoId? MeatSpikePrototype;

    /// <summary>
    /// Optional reagent override for the blood trail on this kit.
    /// If null, the default reagent on SlasherFearComponent is kept.
    /// </summary>
    [DataField]
    public string? BloodTrailReagent;

    /// <summary>
    /// Optional override for the soulsteal sound on this kit.
    /// If null, the default sound on SoulStealComponent is kept.
    /// </summary>
    [DataField]
    public SoundSpecifier? SoulStealSound;

    /// <summary>
    /// Optional starting gear to equip when this kit's slasher ascends.
    /// If null, no clothing swap occurs on ascension.
    /// </summary>
    [DataField]
    public ProtoId<StartingGearPrototype>? AscensionGear;

    /// <summary>
    /// Optional override for the station announcement text key on ascension.
    /// If null, the default slasher-soulsteal-ascendance string is used.
    /// </summary>
    [DataField]
    public string? AscendanceAnnouncementKey;

    /// <summary>
    /// Optional override for the global sound played on ascension.
    /// If null, the default AscendanceSound on SlasherSoulStealComponent is used.
    /// </summary>
    [DataField]
    public SoundSpecifier? AscendanceSound;

    /// <summary>
    /// Identifier this kit records on the player when they ascend with it.
    /// </summary>
    [DataField]
    public string? AscensionId;

    /// <summary>
    /// Prestige requirement: the AscensionId the player must have already
    /// ascended with before this kit can be selected. If null, the kit is always available.
    /// </summary>
    [DataField]
    public string? RequiredAscension;

    /// <summary>
    /// Optional name pool for this kit: the slasher is renamed from these localized dataset
    /// segments on selection (e.g. the Idol's stage names). If null, the spawn name is kept.
    /// </summary>
    [DataField]
    public List<ProtoId<LocalizedDatasetPrototype>>? NameSegments;

    /// <summary>
    /// Format string used to combine NameSegments. Defaults to the standard
    /// slasher name format.
    /// </summary>
    [DataField]
    public LocId NameFormat = "name-format-slasher";

    /// <summary>
    /// Extra components added to the Slasher when this kit is selected. Used for kit-specific
    /// mechanics like the idols star eyes.
    /// </summary>
    [DataField]
    public ComponentRegistry Components = new();

    /// <summary>
    /// Components removed from the Slasher after the shared post-selection and kit components are
    /// added. Used to strip shared abilities (like the incorporeal jaunt) from specific kits.
    /// </summary>
    [DataField]
    public HashSet<string> RemoveComponents = new();

    /// <summary>
    /// Guidebook entry id for this kit. Set on kits whose gameplay differs from the default slasher;
    /// the kit-select card shows a "?" that opens the guidebook to this page.
    /// </summary>
    [DataField]
    public string? Guide;
}

