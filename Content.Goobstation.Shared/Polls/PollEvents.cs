using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Polls;

public sealed class MsgRequestActivePolls : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;
    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer) { }
    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer) { }
}

public sealed class MsgActivePollsResponse : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;
    public List<PollData> Polls { get; set; } = new();

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        var count = buffer.ReadVariableInt32();
        Polls = new List<PollData>(count);
        for (var i = 0; i < count; i++)
            Polls.Add(ReadPollData(buffer));
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.WriteVariableInt32(Polls.Count);
        foreach (var poll in Polls)
            WritePollData(buffer, poll);
    }

    internal static PollData ReadPollData(NetIncomingMessage buffer)
    {
        var poll = new PollData
        {
            PollId = buffer.ReadVariableInt32(),
            Title = buffer.ReadString(),
            Description = buffer.ReadString(),
            StartTime = DateTime.FromBinary(buffer.ReadInt64()),
            Active = buffer.ReadBoolean(),
            AllowMultipleChoices = buffer.ReadBoolean()
        };
        buffer.ReadPadBits();

        var hasEndTime = buffer.ReadBoolean();
        buffer.ReadPadBits();
        if (hasEndTime)
            poll.EndTime = DateTime.FromBinary(buffer.ReadInt64());

        var hasCreator = buffer.ReadBoolean();
        buffer.ReadPadBits();
        if (hasCreator)
            poll.CreatedByName = buffer.ReadString();

        var optionCount = buffer.ReadVariableInt32();
        poll.Options = new List<PollOptionData>(optionCount);
        for (var i = 0; i < optionCount; i++)
        {
            poll.Options.Add(new PollOptionData
            {
                OptionId = buffer.ReadVariableInt32(),
                OptionText = buffer.ReadString(),
                DisplayOrder = buffer.ReadVariableInt32(),
                VoteCount = buffer.ReadVariableInt32()
            });
        }

        return poll;
    }

    internal static void WritePollData(NetOutgoingMessage buffer, PollData poll)
    {
        buffer.WriteVariableInt32(poll.PollId);
        buffer.Write(poll.Title);
        buffer.Write(poll.Description);
        buffer.Write(poll.StartTime.ToBinary());
        buffer.Write(poll.Active);
        buffer.Write(poll.AllowMultipleChoices);
        buffer.WritePadBits();

        buffer.Write(poll.EndTime != null);
        buffer.WritePadBits();
        if (poll.EndTime != null)
            buffer.Write(poll.EndTime.Value.ToBinary());

        buffer.Write(poll.CreatedByName != null);
        buffer.WritePadBits();
        if (poll.CreatedByName != null)
            buffer.Write(poll.CreatedByName);

        buffer.WriteVariableInt32(poll.Options.Count);
        foreach (var option in poll.Options)
        {
            buffer.WriteVariableInt32(option.OptionId);
            buffer.Write(option.OptionText);
            buffer.WriteVariableInt32(option.DisplayOrder);
            buffer.WriteVariableInt32(option.VoteCount);
        }
    }
}

public sealed class MsgRequestPollDetails : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;
    public int PollId { get; set; }

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        PollId = buffer.ReadVariableInt32();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.WriteVariableInt32(PollId);
    }
}

public sealed class MsgPollDetailsResponse : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;
    public PollData? Poll { get; set; }
    public List<PollVoteData> PlayerVotes { get; set; } = new();

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        var hasPoll = buffer.ReadBoolean();
        buffer.ReadPadBits();

        if (hasPoll)
            Poll = MsgActivePollsResponse.ReadPollData(buffer);

        var voteCount = buffer.ReadVariableInt32();
        PlayerVotes = new List<PollVoteData>(voteCount);

        for (var i = 0; i < voteCount; i++)
        {
            PlayerVotes.Add(new PollVoteData
            {
                PollId = buffer.ReadVariableInt32(),
                OptionId = buffer.ReadVariableInt32(),
                VotedAt = DateTime.FromBinary(buffer.ReadInt64())
            });
        }
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(Poll != null);
        buffer.WritePadBits();

        if (Poll != null)
            MsgActivePollsResponse.WritePollData(buffer, Poll);

        buffer.WriteVariableInt32(PlayerVotes.Count);

        foreach (var vote in PlayerVotes)
        {
            buffer.WriteVariableInt32(vote.PollId);
            buffer.WriteVariableInt32(vote.OptionId);
            buffer.Write(vote.VotedAt.ToBinary());
        }
    }
}

public sealed class MsgCastPollVote : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;
    public int PollId { get; set; }
    public int OptionId { get; set; }

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        PollId = buffer.ReadVariableInt32();
        OptionId = buffer.ReadVariableInt32();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.WriteVariableInt32(PollId);
        buffer.WriteVariableInt32(OptionId);
    }
}

public sealed class MsgRemovePollVote : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;
    public int PollId { get; set; }
    public int OptionId { get; set; }

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        PollId = buffer.ReadVariableInt32();
        OptionId = buffer.ReadVariableInt32();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.WriteVariableInt32(PollId);
        buffer.WriteVariableInt32(OptionId);
    }
}

public sealed class MsgPollVoteResponse : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public PollData? UpdatedPoll { get; set; }

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        Success = buffer.ReadBoolean();

        var hasError = buffer.ReadBoolean();
        buffer.ReadPadBits();

        if (hasError)
            ErrorMessage = buffer.ReadString();

        var hasPoll = buffer.ReadBoolean();
        buffer.ReadPadBits();

        if (hasPoll)
            UpdatedPoll = MsgActivePollsResponse.ReadPollData(buffer);
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(Success);
        buffer.Write(ErrorMessage != null);
        buffer.WritePadBits();

        if (ErrorMessage != null)
            buffer.Write(ErrorMessage);

        buffer.Write(UpdatedPoll != null);
        buffer.WritePadBits();

        if (UpdatedPoll != null)
            MsgActivePollsResponse.WritePollData(buffer, UpdatedPoll);
    }
}

public sealed class MsgPollUpdated : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;
    public PollData Poll { get; set; } = default!;
    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        Poll = MsgActivePollsResponse.ReadPollData(buffer);
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        MsgActivePollsResponse.WritePollData(buffer, Poll);
    }
}

public sealed class MsgPollClosed : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;
    public int PollId { get; set; }

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        PollId = buffer.ReadVariableInt32();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.WriteVariableInt32(PollId);
    }
}
