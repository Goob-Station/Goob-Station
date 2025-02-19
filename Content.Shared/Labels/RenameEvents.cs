using Robust.Shared.Serialization;

namespace Content.Client.Labels.RenameSystem;

[Serializable, NetSerializable]
public enum HandRenamerUiKey
{
    Key,
}

[Serializable, NetSerializable]
public sealed class HandRenamerComponentState : IComponentState
{
    public string AssignedName;

    public int MaxNameChars;


    public HandRenamerComponentState(string assignedName, int maxNameChars)
    {
        AssignedName = assignedName;

        MaxNameChars = maxNameChars;
    }
}

[Serializable, NetSerializable]
public sealed class HandRenamerNameChangedMessage(string name) : BoundUserInterfaceMessage
{
    public string Name { get; } = name;
}
