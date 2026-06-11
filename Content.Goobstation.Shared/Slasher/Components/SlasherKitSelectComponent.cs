using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
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
    /// If null, the default sound on SlasherBloodTrailComponent is kept.
    /// </summary>
    [DataField]
    public SoundSpecifier? BloodTrailMusic;

    /// <summary>
    /// Optional jumpscare sound override for the blood trail sound on this kit.
    /// If null, the default sounds on SlasherBloodTrailComponent are kept.
    /// </summary>
    [DataField]
    public SoundSpecifier? JumpscareSound;

    /// <summary>
    /// Optional meat spike prototype override for this kit.
    /// If null, the default SlasherSummonMeatSpikeComponent prototype is kept.
    /// </summary>
    [DataField]
    public EntProtoId? MeatSpikePrototype;

    /// <summary>
    /// Optional reagent override for the blood trail on this kit.
    /// If null, the default reagent on SlasherBloodTrailComponent is kept.
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

}

