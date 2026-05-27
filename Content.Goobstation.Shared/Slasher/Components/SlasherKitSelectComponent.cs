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
    public Dictionary<string, SlasherKit> Kits = new();

    [DataField]
    public ComponentRegistry PostSelectionComponents = new();
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
}

