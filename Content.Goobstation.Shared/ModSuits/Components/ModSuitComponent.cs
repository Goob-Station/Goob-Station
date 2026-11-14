using Content.Goobstation.Shared.ModSuits.Systems;
using Content.Shared.Alert;
using Content.Shared.Inventory;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.ModSuits.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
[Access(typeof(ModSuitSystem))]
public sealed partial class ModSuitComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public ProtoId<ModThemePrototype> Theme;

    [DataField]
    public Dictionary<string, EntProtoId> Parts = new();

    [DataField, AutoNetworkedField]
    public Dictionary<string, EntityUid> PartEntities = new();

    [DataField]
    public List<EntProtoId> StartingModules = new();

    [DataField]
    public List<EntProtoId> InbuiltModules = new();

    [DataField]
    public string PartsContainerId = "mod-parts";

    [DataField]
    public string ModulesContainerId = "mod-modules";

    [DataField]
    public string CoreSlotId = "mod-core";

    [ViewVariables]
    public Container PartsContainer = default!;

    [ViewVariables]
    public Container ModulesContainer = default!;

    [DataField, AutoNetworkedField]
    public ModSuitState State = ModSuitState.Inactive;

    [DataField, AutoNetworkedField]
    public List<string> ActivationQueue = new();

    [DataField, AutoNetworkedField, AutoPausedField]
    public TimeSpan NextActivationStep;

    [DataField, AutoNetworkedField]
    public List<string> DeployQueue = new();

    [DataField, AutoNetworkedField]
    public bool DeployQueueRetracting;

    [DataField, AutoNetworkedField, AutoPausedField]
    public TimeSpan NextDeployStep;

    [DataField]
    public TimeSpan DeployStepTime = TimeSpan.FromSeconds(0.5);

    [DataField, AutoNetworkedField]
    public EntityUid? Wearer;

    [DataField, AutoNetworkedField]
    public bool Powered;

    [DataField]
    public SlotFlags RequiredSlot = SlotFlags.BACK;

    [DataField]
    public float UnpoweredSpeedModifier = 0.7f;

    [DataField]
    public TimeSpan OtherUserToggleTime = TimeSpan.FromSeconds(2);

    [DataField]
    public EntProtoId ActivateAction = "ActionModSuitActivate";

    [DataField, AutoNetworkedField]
    public EntityUid? ActivateActionEntity;

    [DataField]
    public EntProtoId DeployAction = "ActionModSuitDeploy";

    [DataField, AutoNetworkedField]
    public EntityUid? DeployActionEntity;

    [DataField]
    public ProtoId<AlertPrototype> PowerAlert = "ModsuitPower";

    [DataField]
    public SoundSpecifier ActivateSound = new SoundPathSpecifier("/Audio/_Goobstation/Mecha/nominal.ogg");

    [DataField]
    public SoundSpecifier DeactivateSound = new SoundPathSpecifier("/Audio/_Goobstation/Machines/computer_end.ogg");

    [DataField]
    public SoundSpecifier FailSound = new SoundPathSpecifier("/Audio/_Goobstation/Machines/ErrorBeep2.wav");

    [DataField]
    public SoundSpecifier SealBrokenSound = new SoundPathSpecifier("/Audio/_Goobstation/Machines/MaxTempAlertCut.wav");

    [DataField]
    public SoundSpecifier ModuleInsertSound = new SoundPathSpecifier("/Audio/Items/screwdriver2.ogg");

    [DataField]
    public SoundSpecifier ModuleRemoveSound = new SoundPathSpecifier("/Audio/Items/crowbar.ogg");
}

[Serializable, NetSerializable]
public enum ModSuitState : byte
{
    Inactive,
    Activating,
    Active,
    Deactivating,
}

[Serializable, NetSerializable]
public enum ModSuitVisuals : byte
{
    Sealed,
}
