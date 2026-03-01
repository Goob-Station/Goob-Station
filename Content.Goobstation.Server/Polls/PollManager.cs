using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Content.Goobstation.Shared.Polls;
using Content.Server.Database;
using Robust.Server.Player;
using Robust.Shared.Network;

namespace Content.Goobstation.Server.Polls;

public sealed class PollManager : IPostInjectInit
{
    [Dependency] private readonly IServerDbManager _db = null!;
    [Dependency] private readonly IPlayerManager _playerManager = null!;
    [Dependency] private readonly INetManager _net = null!;

    private readonly Dictionary<int, Poll> _cachedPolls = [];
    private readonly object _cacheLock = new();

    public async Task<PollData?> CreatePoll(
        string title,
        string description,
        List<string> options,
        DateTime? endTime,
        bool allowMultipleChoices,
        NetUserId? creatorId = null)
    {
        var poll = new Poll
        {
            Title = title,
            Description = description,
            StartTime = DateTime.UtcNow,
            EndTime = endTime,
            Active = true,
            AllowMultipleChoices = allowMultipleChoices,
            CreatedById = creatorId?.UserId,
            CreatedAt = DateTime.UtcNow,
            Options = []
        };

        for (var i = 0; i < options.Count; i++)
        {
            poll.Options.Add(new PollOption
            {
                OptionText = options[i],
                DisplayOrder = i
            });
        }

        var pollId = await _db.CreatePollAsync(poll);
        poll.Id = pollId;

        lock (_cacheLock)
            _cachedPolls[pollId] = poll;

        var pollData = await ConvertToPollData(poll);

        if (pollData != null)
            BroadcastPollUpdate(pollData);

        return pollData;
    }

    public async Task<List<PollData>> GetActivePolls()
    {
        var polls = await _db.GetAllPollsAsync();
        var pollData = new List<PollData>();

        foreach (var poll in polls)
        {
            var data = await ConvertToPollData(poll);
            if (data != null)
                pollData.Add(data);
        }

        return pollData;
    }

    public async Task<PollData?> GetPoll(int pollId)
    {
        var poll = await _db.GetPollAsync(pollId);
        return poll != null ? await ConvertToPollData(poll) : null;
    }

    public async Task<(bool success, string? error, PollData? poll)> CastVote(int pollId, int optionId, NetUserId userId)
    {
        var poll = await _db.GetPollAsync(pollId);

        if (poll == null)
            return (false, "Poll not found", null);

        if (!poll.Active)
            return (false, "Poll is not active", null);

        if (poll.EndTime < DateTime.UtcNow)
            return (false, "Poll has ended", null);

        if (!poll.Options.Any(o => o.Id == optionId))
            return (false, "Invalid option", null);

        var success = await _db.AddPollVoteAsync(pollId, optionId, userId);

        if (!success)
            return (false, "Failed to cast vote", null);

        var updatedPoll = await GetPoll(pollId);
        return (true, null, updatedPoll);
    }

    public async Task<bool> RemoveVote(int pollId, int optionId, NetUserId userId)
    {
        return await _db.RemovePollVoteAsync(pollId, optionId, userId);
    }

    public async Task<List<PollVoteData>> GetPlayerVotes(int pollId, NetUserId userId)
    {
        var votes = await _db.GetPlayerVotesAsync(pollId, userId);
        return votes.ConvertAll(v => new PollVoteData
        {
            PollId = v.PollId,
            OptionId = v.PollOptionId,
            VotedAt = v.VotedAt
        });
    }

    public async Task ClosePoll(int pollId)
    {
        await _db.UpdatePollStatusAsync(pollId, false);

        lock (_cacheLock)
        {
            if (_cachedPolls.TryGetValue(pollId, out var poll))
                poll.Active = false;
        }

        var closeMsg = new MsgPollClosed { PollId = pollId };
        _net.ServerSendToAll(closeMsg);
    }

    private async Task<PollData?> ConvertToPollData(Poll poll)
    {
        var results = await _db.GetPollResultsAsync(poll.Id);

        string? creatorName = null;
        if (poll.CreatedBy != null)
        {
            var creatorRecord = await _db.GetPlayerRecordByUserId(new NetUserId(poll.CreatedBy.UserId), CancellationToken.None);
            creatorName = creatorRecord?.LastSeenUserName;
        }

        return new PollData
        {
            PollId = poll.Id,
            Title = poll.Title,
            Description = poll.Description,
            StartTime = poll.StartTime,
            EndTime = poll.EndTime,
            Active = poll.Active,
            AllowMultipleChoices = poll.AllowMultipleChoices,
            CreatedByName = creatorName,
            Options = [.. poll.Options.OrderBy(o => o.DisplayOrder).Select(o => new PollOptionData
            {
                OptionId = o.Id,
                OptionText = o.OptionText,
                DisplayOrder = o.DisplayOrder,
                VoteCount = results.TryGetValue(o.Id, out var count) ? count : 0
            })]
        };
    }

    private void BroadcastPollUpdate(PollData poll)
    {
        var updateMsg = new MsgPollUpdated { Poll = poll };
        _net.ServerSendToAll(updateMsg);
    }

    private async void OnRequestActivePolls(MsgRequestActivePolls msg)
    {
        var polls = await GetActivePolls();
        var response = new MsgActivePollsResponse { Polls = polls };
        _net.ServerSendMessage(response, msg.MsgChannel);
    }

    private async void OnRequestPollDetails(MsgRequestPollDetails msg)
    {
        var poll = await GetPoll(msg.PollId);
        var playerVotes = new List<PollVoteData>();

        if (_playerManager.TryGetSessionByChannel(msg.MsgChannel, out var session) && session.UserId != default)
            playerVotes = await GetPlayerVotes(msg.PollId, session.UserId);

        var response = new MsgPollDetailsResponse
        {
            Poll = poll,
            PlayerVotes = playerVotes
        };

        _net.ServerSendMessage(response, msg.MsgChannel);
    }

    private async void OnCastVote(MsgCastPollVote msg)
    {
        if (!_playerManager.TryGetSessionByChannel(msg.MsgChannel, out var session) || session.UserId == default)
        {
            SendVoteResponse(false, "Not logged in", null, msg.MsgChannel);
            return;
        }

        var (success, error, updatedPoll) = await CastVote(msg.PollId, msg.OptionId, session.UserId);
        SendVoteResponse(success, error, updatedPoll, msg.MsgChannel);

        if (success && updatedPoll != null)
            BroadcastPollUpdate(updatedPoll);
    }

    private async void OnRemoveVote(MsgRemovePollVote msg)
    {
        if (!_playerManager.TryGetSessionByChannel(msg.MsgChannel, out var session) || session.UserId == default)
        {
            SendVoteResponse(false, "Not logged in", null, msg.MsgChannel); // is this even possbile really? dunno but might as well check
            return;
        }

        var success = await RemoveVote(msg.PollId, msg.OptionId, session.UserId);
        var updatedPoll = success ? await GetPoll(msg.PollId) : null;

        SendVoteResponse(success, success ? null : "Failed to remove vote", updatedPoll, msg.MsgChannel);

        if (success && updatedPoll != null)
            BroadcastPollUpdate(updatedPoll);
    }

    private void SendVoteResponse(bool success, string? error, PollData? poll, INetChannel channel)
    {
        var response = new MsgPollVoteResponse
        {
            Success = success,
            ErrorMessage = error,
            UpdatedPoll = poll
        };

        _net.ServerSendMessage(response, channel);
    }

    void IPostInjectInit.PostInject()
    {
        _net.RegisterNetMessage<MsgRequestActivePolls>(OnRequestActivePolls);
        _net.RegisterNetMessage<MsgActivePollsResponse>();
        _net.RegisterNetMessage<MsgRequestPollDetails>(OnRequestPollDetails);
        _net.RegisterNetMessage<MsgPollDetailsResponse>();
        _net.RegisterNetMessage<MsgCastPollVote>(OnCastVote);
        _net.RegisterNetMessage<MsgRemovePollVote>(OnRemoveVote);
        _net.RegisterNetMessage<MsgPollVoteResponse>();
        _net.RegisterNetMessage<MsgPollUpdated>();
        _net.RegisterNetMessage<MsgPollClosed>();
    }
}
