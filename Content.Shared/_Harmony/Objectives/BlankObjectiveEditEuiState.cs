using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared.Objectives;

[Serializable, NetSerializable]
public sealed class ObjetiveSaveMessage : EuiMessageBase
{
    public string Name { get; }
    public string Description { get; }
    public ObjetiveSaveMessage(string name, string description)
    {
        Name = name;
        Description = description;
    }
}
