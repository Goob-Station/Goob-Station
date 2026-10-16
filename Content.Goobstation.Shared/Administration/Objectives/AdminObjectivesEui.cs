using Content.Shared.Eui;
using Content.Shared.Objectives;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Administration.Objectives;

public static class AdminObjectivesLimits
{
    public const int MaxTitleLength = 150;
    public const int MaxDescriptionLength = 500;
}

/// <summary>
/// An objective currently held by the edited mind.
/// </summary>
[Serializable, NetSerializable]
public sealed record AdminObjectiveEntry(
    NetEntity Objective,
    string? ProtoId,
    ObjectiveInfo? Info,
    string Issuer,
    string? TargetName);

/// <summary>
/// An objective prototype the admin may add. <see cref="HasTarget"/> means a target can be designated.
/// </summary>
[Serializable, NetSerializable]
public sealed record AdminObjectivePrototypeEntry(string Id, string Name, bool HasTarget);

/// <summary>
/// A mind that can be designated as the target of a kill / teach a lesson / similar objective.
/// </summary>
[Serializable, NetSerializable]
public sealed record AdminObjectiveTargetEntry(NetEntity Mind, string CharacterName, string Job, bool Dead);

/// <summary>
/// An issuer (header an objective is grouped under) that the admin can pick.
/// <see cref="Id"/> is the locale id, <see cref="Name"/> the localized text with markup stripped.
/// </summary>
[Serializable, NetSerializable]
public sealed record AdminObjectiveIssuerEntry(string Id, string Name);

[Serializable, NetSerializable]
public sealed class AdminObjectivesEuiState : EuiStateBase
{
    public string PlayerName { get; }
    public bool HasMind { get; }
    public List<AdminObjectiveEntry> Objectives { get; }
    public List<AdminObjectivePrototypeEntry> Prototypes { get; }
    public List<AdminObjectiveTargetEntry> Targets { get; }
    public List<AdminObjectiveIssuerEntry> Issuers { get; }

    public AdminObjectivesEuiState(
        string playerName,
        bool hasMind,
        List<AdminObjectiveEntry> objectives,
        List<AdminObjectivePrototypeEntry> prototypes,
        List<AdminObjectiveTargetEntry> targets,
        List<AdminObjectiveIssuerEntry> issuers)
    {
        PlayerName = playerName;
        HasMind = hasMind;
        Objectives = objectives;
        Prototypes = prototypes;
        Targets = targets;
        Issuers = issuers;
    }
}

/// <summary>
/// Add an objective from a prototype, optionally with a designated target mind.
/// </summary>
[Serializable, NetSerializable]
public sealed class AdminObjectivesAddPrototypeMessage : EuiMessageBase
{
    public string ProtoId { get; }
    public NetEntity? TargetMind { get; }
    public bool Bypass { get; }

    public AdminObjectivesAddPrototypeMessage(string protoId, NetEntity? targetMind, bool bypass)
    {
        ProtoId = protoId;
        TargetMind = targetMind;
        Bypass = bypass;
    }
}

/// <summary>
/// Add a custom objective that is always complete and shows the admin's text.
/// </summary>
[Serializable, NetSerializable]
public sealed class AdminObjectivesAddCustomMessage : EuiMessageBase
{
    public string Title { get; }
    public string Description { get; }
    public string Issuer { get; }

    public AdminObjectivesAddCustomMessage(string title, string description, string issuer)
    {
        Title = title;
        Description = description;
        Issuer = issuer;
    }
}

/// <summary>
/// Add a steal objective for one specific entity.
/// </summary>
[Serializable, NetSerializable]
public sealed class AdminObjectivesAddStealMessage : EuiMessageBase
{
    public NetEntity Item { get; }
    public string Issuer { get; }

    public AdminObjectivesAddStealMessage(NetEntity item, string issuer)
    {
        Item = item;
        Issuer = issuer;
    }
}

[Serializable, NetSerializable]
public sealed class AdminObjectivesRemoveMessage : EuiMessageBase
{
    public NetEntity Objective { get; }

    public AdminObjectivesRemoveMessage(NetEntity objective)
    {
        Objective = objective;
    }
}

[Serializable, NetSerializable]
public sealed class AdminObjectivesRefreshMessage : EuiMessageBase;

/// <summary>
/// Server to client result of the last action, shown in the window's status line.
/// </summary>
[Serializable, NetSerializable]
public sealed class AdminObjectivesResultMessage : EuiMessageBase
{
    public string Message { get; }
    public bool Success { get; }

    public AdminObjectivesResultMessage(string message, bool success)
    {
        Message = message;
        Success = success;
    }
}
