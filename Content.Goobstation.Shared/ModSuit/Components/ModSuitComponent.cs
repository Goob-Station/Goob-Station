using Robust.Shared.Audio;
using Content.Shared.Inventory;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.ModSuits;

/// <summary>
///     This component gives an item an action that will equip or un-equip some clothing e.g. hardsuits and hardsuit helmets.
/// </summary>

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModSuitComponent : Component
{
    #region GUI

    [DataField]
    public string BackgroundPath = "/Textures/_Goobstation/Interface/Backgrounds/Modsuits/nanotrasen_background.png";

    [DataField]
    public Color BackpanelsColor = new Color(0.06f, 0.1f, 0.16f, 0.6f);

    [DataField]
    public Color ScrollColor = new Color(0.06f, 0.1f, 0.16f, 0.6f);

    [DataField]
    public List<Color> ButtonColors = new() { Color.FromHex("#121923ff"), Color.FromHex("#04060aFF"), Color.FromHex("#153b66"), Color.FromHex("#153b66") };

    #endregion gui

    #region Containers

    public const string DefaultClothingContainerId = "modsuit-part";
    public const string DefaultModuleContainerId = "mod-modules-container";

    /// <summary>
    ///     The container that the clothing is stored in when not equipped.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string ContainerId = DefaultClothingContainerId;

    [DataField, AutoNetworkedField]
    public string ModuleContainerId = DefaultModuleContainerId;

    [ViewVariables]
    public Container PartsContainer = default!;

    [ViewVariables(VVAccess.ReadWrite)]
    public Container ModuleContainer = default!;

    #endregion

    #region Actions

    /// <summary>
    /// Action used to toggle the clothing on or off.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId Action = "ActionToggleModPart";

    [DataField, AutoNetworkedField]
    public EntProtoId MenuAction = "ActionToggleModMenu";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;

    [DataField, AutoNetworkedField]
    public EntityUid? ActionMenuEntity;

    #endregion

    #region Stats
    /// <summary>
    /// Maximum modules complexity
    /// </summary>
    [DataField, AutoNetworkedField]
    public int MaxComplexity = 15;

    /// <summary>
    /// Non-modified energy using. 1 toggled part - 1 energy per PowerCellDraw use
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ModEnergyBaseUsing = 0.5f;

    /// <summary>
    ///     Dictionary of inventory slots and entity prototypes to spawn into the clothing container.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<string, EntProtoId> ClothingPrototypes = new();

    /// <summary>
    ///     Dictionary of clothing uids and slots
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<NetEntity, string> ClothingUids = new();

    /// <summary>
    ///     Time it takes for this clothing to be toggled via the stripping menu verbs. Null prevents the verb from even showing up.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan? StripDelay = TimeSpan.FromSeconds(3);

    #endregion

    #region Sounds

    /// <summary>
    /// Sound, playing when mod is fully enabled
    /// </summary>
    [DataField]
    public SoundSpecifier FullyEnabledSound = new SoundPathSpecifier("/Audio/_Goobstation/Mecha/nominal.ogg");

    [DataField]
    public SoundSpecifier InsertModuleSound = new SoundPathSpecifier("/Audio/Machines/id_insert.ogg");

    [DataField]
    public SoundSpecifier EjectModuleSound = new SoundPathSpecifier("/Audio/Machines/id_swipe.ogg");

    #endregion
    /// <summary>
    ///     Text shown in the toggle-clothing verb. Defaults to using the name of the <see cref="ActionEntity"/> action.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? VerbText;

    /// <summary>
    ///     If true it will block unequip of this entity until all attached clothing are removed
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool BlockUnequipWhenAttached = true;

    /// <summary>
    ///     If true all attached will replace already equipped clothing on equip attempt
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ReplaceCurrentClothing = true;

    [DataField("requiredSlot"), AutoNetworkedField]
    public SlotFlags RequiredFlags = SlotFlags.BACK;

    /// <summary>
    ///     Modules on start
    /// </summary>
    [DataField]
    public List<EntProtoId> StartingModules = [];

    [AutoNetworkedField]
    public int CurrentComplexity = 0;

    [AutoNetworkedField]
    public string? UserName = null;

    [AutoNetworkedField]
    public EntityUid? TempUser = null;
}

[Serializable, NetSerializable]
public enum ModSuitMenuUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public enum ModSuitUiKey : byte
{
    Key
}
