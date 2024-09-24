using Content.Shared.Construction.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.ShortConstruction;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShortConstructionComponent : Component
{
    [DataField(required: true)]
    public List<ShortConstructionEntry> Entries = new();
}

[DataDefinition]
public sealed partial class ShortConstructionEntry
{
    [DataField]
    public ProtoId<ConstructionPrototype>? Prototype { get; set; }

    [DataField]
    public ShortConstructionCategory? Category { get; set; }
}

[DataDefinition]
public sealed partial class ShortConstructionCategory
{
    [DataField]
    public string Name { get; set; } = string.Empty;

    [DataField]
    public SpriteSpecifier Icon { get; set; } = default!;

    [DataField]
    public List<ShortConstructionEntry> Entries { get; set; } = new();
}

[NetSerializable, Serializable]
public enum ShortConstructionUiKey : byte
{
    Key,
}
