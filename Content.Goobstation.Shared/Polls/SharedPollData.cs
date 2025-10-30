using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Polls;

[Serializable, NetSerializable]
public sealed class PollData
{
    public int PollId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool Active { get; set; }
    public bool AllowMultipleChoices { get; set; }
    public List<PollOptionData> Options { get; set; } = [];
    public string? CreatedByName { get; set; }
}

[Serializable, NetSerializable]
public sealed class PollOptionData
{
    public int OptionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public int VoteCount { get; set; }
}

[Serializable, NetSerializable]
public sealed class PollVoteData
{
    public int PollId { get; set; }
    public int OptionId { get; set; }
    public DateTime VotedAt { get; set; }
}
