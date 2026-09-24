using Content.Shared.Chemistry.Reagent;
using Content.Shared.Guidebook;
using Content.Shared.Roles;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Allows the Slasher to choose their kit (starting gear) when they first spawn.
/// </summary>
[RegisterComponent]
public sealed partial class SlasherKitSelectComponent : Component
{
    [DataField]
    public bool KitSelected;

    /// <summary>
    /// The kits on offer, keyed by the locale key of the kit's name.
    /// </summary>
    [DataField(required: true)]
    public Dictionary<string, SlasherKit> Kits = [];

    /// <summary>
    /// Default song for the trailer music.
    /// </summary>
    [DataField]
    public SoundSpecifier DefaultThemeSong = new SoundPathSpecifier(
        "/Audio/_Goobstation/Slasher/Music/slasher_serial_killer_murder_frenzy_insane_horror_soundtrack.ogg");

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
    /// The kit description shown in the UI.
    /// </summary>
    [DataField]
    public LocId? Description;

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
    /// Optional extra component added alongside the fear status effect.
    /// If null, nothing gets added.
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
    public ProtoId<ReagentPrototype>? BloodTrailReagent;

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
    /// Optional override for the station announcement on ascension.
    /// If null, the default slasher-soulsteal-ascendance string is used.
    /// </summary>
    [DataField]
    public LocId? AscendanceAnnouncementKey;

    /// <summary>
    /// Optional override for the global sound played on ascension.
    /// If null, the default AscendanceSound on SlasherSoulStealComponent is used.
    /// </summary>
    [DataField]
    public SoundSpecifier? AscendanceSound;

    /// <summary>
    /// Optional marker that saves to the users profile for prestiges.
    /// </summary>
    [DataField]
    public string? AscensionId;

    /// <summary>
    /// Is this kit a prestige? If so what ascension ID does it require?
    /// </summary>
    [DataField]
    public string? RequiredAscension;

    /// <summary>
    /// Extra components added to the Slasher when this kit is selected. Used for kit-specific
    /// mechanics like the boogeyman's shadow invisibility.
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
    /// Guidebook entry for this kit. Set on kits whose gameplay differs from the default slasher;
    /// the kit-select card gets a button that opens the guidebook to this page.
    /// </summary>
    [DataField]
    public ProtoId<GuideEntryPrototype>? Guide;
}
