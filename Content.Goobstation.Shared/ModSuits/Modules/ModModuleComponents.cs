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
