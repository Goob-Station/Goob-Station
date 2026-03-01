using System.Linq;
using Content.Goobstation.Shared.Polls;
using Robust.Shared.Network;

namespace Content.Goobstation.Client.Polls;

public sealed class PollManager
{
    [Dependency] private readonly IClientNetManager _net = default!;

    private readonly Dictionary<int, PollData> _activePolls = [];
    private readonly Dictionary<int, List<PollVoteData>> _playerVotes = [];

    public event Action<List<PollData>>? OnActivePollsUpdated;
    public event Action<PollData>? OnPollUpdated;
    public event Action<int>? OnPollClosed;
    public event Action<PollData, List<PollVoteData>>? OnPollDetailsReceived;
    public event Action<bool, string?>? OnVoteResponse;

    public IReadOnlyDictionary<int, PollData> ActivePolls => _activePolls;

    public void Initialize()
    {
        _net.RegisterNetMessage<MsgRequestActivePolls>();
        _net.RegisterNetMessage<MsgActivePollsResponse>(HandleActivePollsResponse);
        _net.RegisterNetMessage<MsgRequestPollDetails>();
        _net.RegisterNetMessage<MsgPollDetailsResponse>(HandlePollDetailsResponse);
        _net.RegisterNetMessage<MsgCastPollVote>();
        _net.RegisterNetMessage<MsgRemovePollVote>();
        _net.RegisterNetMessage<MsgPollVoteResponse>(HandlePollVoteResponse);
        _net.RegisterNetMessage<MsgPollUpdated>(HandlePollUpdated);
        _net.RegisterNetMessage<MsgPollClosed>(HandlePollClosed);
    }

    public void RequestActivePolls()
    {
        var msg = new MsgRequestActivePolls();
        _net.ClientSendMessage(msg);
    }

    public void RequestPollDetails(int pollId)
    {
        var msg = new MsgRequestPollDetails { PollId = pollId };
        _net.ClientSendMessage(msg);
    }

    public void CastVote(int pollId, int optionId)
    {
        if (!_playerVotes.ContainsKey(pollId))
            _playerVotes[pollId] = [];

        if (_activePolls.TryGetValue(pollId, out var poll) && !poll.AllowMultipleChoices)
            _playerVotes[pollId].Clear();

        if (!_playerVotes[pollId].Any(v => v.OptionId == optionId))
        {
            _playerVotes[pollId].Add(new PollVoteData
            {
                PollId = pollId,
                OptionId = optionId,
                VotedAt = DateTime.UtcNow
            });
        }

        var msg = new MsgCastPollVote
        {
            PollId = pollId,
            OptionId = optionId
        };
        _net.ClientSendMessage(msg);
    }

    public void RemoveVote(int pollId, int optionId)
    {
        if (_playerVotes.TryGetValue(pollId, out var value))
            value.RemoveAll(v => v.OptionId == optionId);

        var msg = new MsgRemovePollVote
        {
            PollId = pollId,
            OptionId = optionId
        };

        _net.ClientSendMessage(msg);
    }

    public List<PollVoteData> GetPlayerVotes(int pollId)
    {
        return _playerVotes.TryGetValue(pollId, out var votes) ? votes : [];
    }

    public bool HasVotedForOption(int pollId, int optionId)
    {
        if (!_playerVotes.TryGetValue(pollId, out var votes))
            return false;

        return votes.Any(v => v.OptionId == optionId);
    }

    private void HandleActivePollsResponse(MsgActivePollsResponse msg)
    {
        _activePolls.Clear();

        foreach (var poll in msg.Polls)
            _activePolls[poll.PollId] = poll;

        OnActivePollsUpdated?.Invoke(msg.Polls);
    }

    private void HandlePollDetailsResponse(MsgPollDetailsResponse msg)
    {
        if (msg.Poll != null)
        {
            _activePolls[msg.Poll.PollId] = msg.Poll;
            _playerVotes[msg.Poll.PollId] = msg.PlayerVotes;

            OnPollDetailsReceived?.Invoke(msg.Poll, msg.PlayerVotes);
        }
    }

    private void HandlePollVoteResponse(MsgPollVoteResponse msg)
    {
        if (msg.Success && msg.UpdatedPoll != null)
            _activePolls[msg.UpdatedPoll.PollId] = msg.UpdatedPoll;

        OnVoteResponse?.Invoke(msg.Success, msg.ErrorMessage);
    }

    private void HandlePollUpdated(MsgPollUpdated msg)
    {
        _activePolls[msg.Poll.PollId] = msg.Poll;
        OnPollUpdated?.Invoke(msg.Poll);
    }

    private void HandlePollClosed(MsgPollClosed msg)
    {
        if (_activePolls.TryGetValue(msg.PollId, out var value))
            value.Active = false;

        OnPollClosed?.Invoke(msg.PollId);
    }
}
