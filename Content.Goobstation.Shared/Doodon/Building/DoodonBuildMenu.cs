using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Doodon.Building;

[Serializable, NetSerializable]
public enum DoodonBuildUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class DoodonBuildMenuEntry
{
    public readonly string PrototypeId;
    public readonly string Name;
    public readonly SpriteSpecifier? Icon;
    public readonly int ResinCost;
    public readonly string? Description;

    public DoodonBuildMenuEntry(string prototypeId, string name, SpriteSpecifier? icon, int resinCost, string? description)
    {
        PrototypeId = prototypeId;
        Name = name;
        Icon = icon;
        ResinCost = resinCost;
        Description = description;
    }
}

[Serializable, NetSerializable]
public sealed class DoodonBuildMenuState : BoundUserInterfaceState
{
    public readonly DoodonBuildMenuEntry[] Entries;
    public readonly int SelectedIndex;

    public DoodonBuildMenuState(DoodonBuildMenuEntry[] entries, int selectedIndex)
    {
        Entries = entries;
        SelectedIndex = selectedIndex;
    }
}

[Serializable, NetSerializable]
public sealed class DoodonBuildSelectMessage : BoundUserInterfaceMessage
{
    public readonly string PrototypeId;

    public DoodonBuildSelectMessage(string prototypeId)
    {
        PrototypeId = prototypeId;
    }
}
