using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._White.RadialSelector;

[NetSerializable, Serializable]
public enum RadialSelectorUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class RadialSelectorState(List<RadialSelectorEntry> entries, bool openCentered = false)
    : BoundUserInterfaceState
{
    [DataField(required: true)]
    public List<RadialSelectorEntry> Entries = entries;

    public bool OpenCentered { get; } = openCentered;
}

[Serializable, NetSerializable]
public sealed class TrackedRadialSelectorState(List<RadialSelectorEntry> entries, NetEntity? trackedEntity = null)
    : BoundUserInterfaceState
{
    [DataField(required: true)]
    public List<RadialSelectorEntry> Entries = entries;

    public NetEntity? TrackedEntity { get; } = trackedEntity;
}

[Serializable, NetSerializable]
public sealed class RadialSelectorSelectedMessage(string selectedItem) : BoundUserInterfaceMessage
{
    public string SelectedItem { get; private set; } = selectedItem;
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class RadialSelectorEntry
{
    [DataField]
    public string? Prototype { get; set; }

    [DataField]
    public SpriteSpecifier? Icon { get; set; }

    [DataField]
    public RadialSelectorCategory? Category { get; set; }

    [DataField]
    public bool CloseUiOnSelect = true;
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class RadialSelectorCategory
{
    [DataField(required: true)]
    public string Name { get; set; } = string.Empty;

    [DataField(required: true)]
    public SpriteSpecifier Icon { get; set; } = default!;

    [DataField(required: true)]
    public List<RadialSelectorEntry> Entries { get; set; } = new();
}
