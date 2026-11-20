using Content.Shared.Chemistry.Components;
using Content.Shared.Damage;
using Content.Shared.Item;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.ModSuits.Modules;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModGrantComponentsModuleComponent : Component
{
    [DataField(required: true)]
    public ComponentRegistry Components = new();

    [DataField]
    public ModGrantTarget Target = ModGrantTarget.Part;

    [DataField]
    public string? Slot;

    [DataField]
    public bool ToggleItem;

    [DataField, AutoNetworkedField]
    public EntityUid? GrantedTo;
}

[Serializable, NetSerializable]
public enum ModGrantTarget : byte
{
    Part,
    Suit,
    Wearer,
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModStorageModuleComponent : Component
{
    [DataField(required: true)]
    public ComponentRegistry Components = new();
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModLightModuleComponent : Component
{
    [DataField]
    public string Slot = "head";

    [DataField]
    public float? Radius;

    [DataField]
    public float? Energy;

    [DataField]
    public Color? Color;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModDeviceModuleComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public EntProtoId Device;

    [DataField, AutoNetworkedField]
    public EntityUid? DeviceEntity;

    [DataField]
    public string ContainerId = "mod-device";

    [ViewVariables]
    public ContainerSlot DeviceContainer = default!;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModDeviceItemComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Module;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModJetpackModuleComponent : Component;

[RegisterComponent, NetworkedComponent]
public sealed partial class ModEmpShieldModuleComponent : Component
{
    [DataField]
    public bool RequireActive = true;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModMouthholeModuleComponent : Component
{
    [DataField]
    public string Slot = "head";
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModThermalRegulatorModuleComponent : Component
{
    [DataField]
    public float TargetTemperature = 310.15f;

    [DataField]
    public float Rate = 5f;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModOpenUiModuleComponent : Component
{
    [DataField(required: true)]
    public Enum Key = default!;

    [DataField]
    public string? Slot;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModArmorModuleComponent : Component
{
    [DataField(required: true)]
    public DamageModifierSet Modifiers = new();

    [DataField]
    public List<string> Slots = new();
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModTrayScannerModuleComponent : Component
{
    [ViewVariables]
    public EntityUid? User;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModLoudspeakerModuleComponent : Component
{
    [ViewVariables]
    public EntityUid? User;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModHolsterModuleComponent : Component
{
    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public string ContainerId = "mod-holster";

    [ViewVariables]
    public ContainerSlot Container = default!;

    [DataField]
    public SoundSpecifier? HolsterSound = new SoundPathSpecifier("/Audio/Weapons/Guns/MagIn/pistol_magin.ogg");

    [DataField]
    public SoundSpecifier? DrawSound = new SoundPathSpecifier("/Audio/Weapons/Guns/MagOut/pistol_magout.ogg");
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModSoundModuleComponent : Component
{
    [DataField(required: true)]
    public SoundSpecifier Sound = default!;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModSpawnModuleComponent : Component
{
    [DataField]
    public EntProtoId? Prototype;

    [DataField]
    public List<EntProtoId> RandomPrototypes = new();

    [DataField]
    public bool ToHand;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModQuickCarryComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Multiplier = 0.5f;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModQuickCuffComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Multiplier = 0.5f;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModShoveBlockerComponent : Component;

[RegisterComponent, NetworkedComponent]
public sealed partial class ModAntiGravityComponent : Component;

[RegisterComponent, NetworkedComponent]
public sealed partial class ModClampComponent : Component
{
    [DataField]
    public int MaxContents = 3;

    [DataField]
    public TimeSpan GrabDelay = TimeSpan.FromSeconds(2.5);

    [DataField]
    public TimeSpan DropDelay = TimeSpan.FromSeconds(1);

    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public string ContainerId = "mod-clamp";

    [ViewVariables]
    public Container Container = default!;

    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/Mecha/sound_mecha_hydraulic.ogg");
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModPatientTransportComponent : Component
{
    [DataField]
    public EntProtoId BagPrototype = "ModsuitHardlightBag";

    [DataField]
    public bool RequireRestrained;

    [DataField]
    public TimeSpan CaptureDelay = TimeSpan.FromSeconds(1.5);

    [DataField, AutoNetworkedField]
    public EntityUid? LinkedBag;

    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/Weapons/egloves.ogg");
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModHardlightBagComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Projector;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModCaptureBagComponent : Component
{
    [DataField]
    public ComponentRegistry CaptiveComponents = new();

    [ViewVariables]
    public Dictionary<EntityUid, List<string>> Added = new();
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModStasisContainerComponent : Component
{
    [DataField]
    public float Multiplier = 10f;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModInStasisComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Multiplier = 10f;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModSpeedModuleComponent : Component
{
    [DataField]
    public float WalkModifier = 1f;

    [DataField]
    public float SprintModifier = 1f;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModStripComponentsModuleComponent : Component
{
    [DataField(required: true)]
    public List<string> Components = new();
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class ModEnergyShieldModuleComponent : Component
{
    [DataField]
    public int MaxCharges = 1;

    [DataField, AutoNetworkedField]
    public int Charges = 1;

    [DataField]
    public TimeSpan RechargeDelay = TimeSpan.FromSeconds(10);

    [DataField]
    public TimeSpan ChargeIncrement = TimeSpan.FromSeconds(1);

    [DataField, AutoNetworkedField, AutoPausedField]
    public TimeSpan NextCharge;

    [DataField]
    public float BlockPower = 30f;

    [DataField]
    public SoundSpecifier? BlockSound = new SoundPathSpecifier("/Audio/Weapons/block_metal1.ogg");
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModPlateCompressionModuleComponent : Component
{
    [DataField]
    public ProtoId<ItemSizePrototype> Size = "Normal";
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModChameleonModuleComponent : Component
{
    [DataField(required: true)]
    public ComponentRegistry Components = new();

    [ViewVariables]
    public bool Granted;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModEmpPulseModuleComponent : Component
{
    [DataField]
    public float Range = 4f;

    [DataField]
    public float EnergyConsumption = 100000f;

    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(30);
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModCloakModuleComponent : Component;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModCloakedComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Module;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModInjectModuleComponent : Component
{
    [DataField(required: true)]
    public Solution Solution = new();

    [DataField]
    public bool ClearStuns;

    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/Items/hypospray.ogg");
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModDnaLockModuleComponent : Component
{
    [DataField, AutoNetworkedField]
    public string? LockedDna;

    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/Machines/id_swipe.ogg");
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModPowerKickModuleComponent : Component
{
    [DataField(required: true)]
    public DamageSpecifier Damage = new();

    [DataField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(2);

    [DataField]
    public TimeSpan ChargeTime = TimeSpan.FromSeconds(1);

    [DataField]
    public float ThrowSpeed = 12f;

    [DataField]
    public SoundSpecifier? ChargeSound = new SoundPathSpecifier("/Audio/Mecha/sound_mecha_hydraulic.ogg");

    [DataField]
    public SoundSpecifier? HitSound = new SoundPathSpecifier("/Audio/Weapons/punch3.ogg");

    [DataField]
    public SoundSpecifier? LaunchSound;

    [DataField]
    public LocId ChargePopup = "modsuit-power-kick-charge";
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModPowerKickingComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Module;
}
