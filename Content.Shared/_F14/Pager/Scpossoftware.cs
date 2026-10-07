
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._F14.SCPOS;


[Prototype("scposSoftware")]
public sealed partial class SCPOSSoftwarePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public string Name { get; private set; } = default!;

    [DataField(required: true)]
    public string Description { get; private set; } = default!;

    [DataField(required: true)]
    public int DiskCost { get; private set; }

    [DataField]
    public List<string> RequiredAccess { get; private set; } = new();

    [DataField]
    public string? UnlocksProgram { get; private set; }

    [DataField]
    public string Icon { get; private set; } = "/Textures/_F14/Interface/SCPOS/icon_default.png";

    [DataField]
    public bool Core { get; private set; } = false;
}

[Serializable, NetSerializable]
public sealed class SCPOSSoftwareStoreState : BoundUserInterfaceState
{
    public int DiskUsed;
    public int DiskCapacity;
    public List<string> InstalledSoftware = new();
    public List<string> CurrentAccessTags = new();

    public string? InsertedIdName;
}

[Serializable, NetSerializable]
public sealed class SCPOSDownloadSoftwareMessage(string softwareId) : BoundUserInterfaceMessage
{
    public readonly string SoftwareId = softwareId;
}

[Serializable, NetSerializable]
public sealed class SCPOSUninstallSoftwareMessage(string softwareId) : BoundUserInterfaceMessage
{
    public readonly string SoftwareId = softwareId;
}

[Serializable, NetSerializable]
public enum SCPOSSoftwareUiKey : byte
{
    Key
}
