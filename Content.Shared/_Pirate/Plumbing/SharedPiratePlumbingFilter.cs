using Robust.Shared.Serialization;

namespace Content.Shared._Pirate.Plumbing;

/// <summary>
///     UI key for the plumbing filter interface.
/// </summary>
[Serializable, NetSerializable]
public enum PiratePlumbingFilterUiKey : byte
{
    Key,
}

/// <summary>
///     State sent to the client to update the filter UI.
/// </summary>
[Serializable, NetSerializable]
public sealed class PiratePlumbingFilterBoundUserInterfaceState : BoundUserInterfaceState
{
    /// <summary>
    ///     The reagent IDs currently being filtered.
    /// </summary>
    public HashSet<string> FilteredReagents { get; }

    /// <summary>
    ///     Whether the filter is enabled.
    /// </summary>
    public bool Enabled { get; }

    public PiratePlumbingFilterBoundUserInterfaceState(HashSet<string> filteredReagents, bool enabled)
    {
        FilteredReagents = filteredReagents;
        Enabled = enabled;
    }
}

/// <summary>
///     Message to toggle the filter on/off.
/// </summary>
[Serializable, NetSerializable]
public sealed class PiratePlumbingFilterToggleMessage : BoundUserInterfaceMessage
{
    public bool Enabled { get; }

    public PiratePlumbingFilterToggleMessage(bool enabled)
    {
        Enabled = enabled;
    }
}

/// <summary>
///     Message to add a reagent to the filter list.
/// </summary>
[Serializable, NetSerializable]
public sealed class PiratePlumbingFilterAddReagentMessage : BoundUserInterfaceMessage
{
    public string ReagentId { get; }

    public PiratePlumbingFilterAddReagentMessage(string reagentId)
    {
        ReagentId = reagentId;
    }
}

/// <summary>
///     Message to remove a reagent from the filter list.
/// </summary>
[Serializable, NetSerializable]
public sealed class PiratePlumbingFilterRemoveReagentMessage : BoundUserInterfaceMessage
{
    public string ReagentId { get; }

    public PiratePlumbingFilterRemoveReagentMessage(string reagentId)
    {
        ReagentId = reagentId;
    }
}

/// <summary>
///     Message to clear all filtered reagents.
/// </summary>
[Serializable, NetSerializable]
public sealed class PiratePlumbingFilterClearMessage : BoundUserInterfaceMessage;
