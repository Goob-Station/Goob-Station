using Content.Shared.Damage;
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
    [DataField(required: true)]
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
public sealed partial class ModEmpShieldModuleComponent : Component;

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
    [DataField(required: true)]
    public EntProtoId Prototype;
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
