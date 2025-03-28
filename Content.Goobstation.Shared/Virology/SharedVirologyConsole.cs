using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Virology;

[Serializable, NetSerializable]
public sealed class DiseaseInformation
{
    public DiseaseInformation()
    {
        // TODO virology
    }

    // TODO virology
}

public static class VirologyConsoleConstants
{
    public const string CmdAddDisease = "add_disease";
    public const string CmdRenameDisease = "rename_disease";

    // TODO virology

    ///Used by the VirologyServerSystem to send the info about all diseases to each virology console
    public const string NET_STATUS_COLLECTION = "virology-collection";
}

[Serializable, NetSerializable]
public enum VirologyConsoleUIKey
{
    Key
}

[Serializable, NetSerializable]
public sealed class VirologyConsoleState : BoundUserInterfaceState
{
    public List<DiseaseInformation> Info;

    public VirologyConsoleState(List<DiseaseInformation> info)
    {
        Info = info;
    }
}
