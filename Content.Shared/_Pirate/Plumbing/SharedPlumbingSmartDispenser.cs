using Content.Shared.Chemistry;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Serialization;

namespace Content.Shared._Pirate.Plumbing;

[Serializable, NetSerializable]
public enum PlumbingSmartDispenserUiKey : byte
{
    Key,
}

/// <summary>
/// A single reagent entry for the plumbing smart dispenser UI.
/// </summary>
[Serializable, NetSerializable]
public sealed class PlumbingSmartDispenserReagentEntry
{
    public readonly string ReagentId;
    public readonly string LocalizedName;
    public readonly FixedPoint2 Quantity;
    public readonly Color Color;

    public PlumbingSmartDispenserReagentEntry(string reagentId, string localizedName, FixedPoint2 quantity, Color color)
    {
        ReagentId = reagentId;
        LocalizedName = localizedName;
        Quantity = quantity;
        Color = color;
    }
}

/// <summary>
/// BUI state sent to the client containing the current reagent inventory.
/// </summary>
[Serializable, NetSerializable]
public sealed class PlumbingSmartDispenserBuiState : BoundUserInterfaceState
{
    public readonly List<PlumbingSmartDispenserReagentEntry> Entries;
    public readonly float MaxPerReagent;

    public PlumbingSmartDispenserBuiState(
        List<PlumbingSmartDispenserReagentEntry> entries,
        float maxPerReagent)
    {
        Entries = entries;
        MaxPerReagent = maxPerReagent;
    }
}

[Serializable, NetSerializable]
public sealed class PlumbingSmartDispenserActorStateMessage : BoundUserInterfaceMessage
{
    public readonly bool ShowDispensePane;
    public readonly ContainerInfo? OutputContainer;
    public readonly NetEntity? OutputContainerEntity;
    public readonly ReagentDispenserDispenseAmount SelectedDispenseAmount;

    public PlumbingSmartDispenserActorStateMessage(
        bool showDispensePane,
        ContainerInfo? outputContainer,
        NetEntity? outputContainerEntity,
        ReagentDispenserDispenseAmount selectedDispenseAmount)
    {
        ShowDispensePane = showDispensePane;
        OutputContainer = outputContainer;
        OutputContainerEntity = outputContainerEntity;
        SelectedDispenseAmount = selectedDispenseAmount;
    }
}

[Serializable, NetSerializable]
public sealed class PlumbingSmartDispenserRequestActorStateMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class PlumbingSmartDispenserSetDispenseAmountMessage : BoundUserInterfaceMessage
{
    public readonly ReagentDispenserDispenseAmount DispenseAmount;

    public PlumbingSmartDispenserSetDispenseAmountMessage(ReagentDispenserDispenseAmount dispenseAmount)
    {
        DispenseAmount = dispenseAmount;
    }

    public PlumbingSmartDispenserSetDispenseAmountMessage(string value)
    {
        DispenseAmount = new ReagentDispenserSetDispenseAmountMessage(value).ReagentDispenserDispenseAmount;
    }
}

[Serializable, NetSerializable]
public sealed class PlumbingSmartDispenserDispenseReagentMessage : BoundUserInterfaceMessage
{
    public readonly string ReagentId;

    public PlumbingSmartDispenserDispenseReagentMessage(string reagentId)
    {
        ReagentId = reagentId;
    }
}
