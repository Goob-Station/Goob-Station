
using Content.Server.Popups;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.GameTicking;
using Content.Shared.PlayerVoting;
using Content.Shared.Popups;
using Robust.Shared.Network;


namespace Content.Server._TBDStation.ServerKarma;

public sealed class ServerPlayerVotingSystem : EntitySystem
{
    [Dependency] private readonly ServerKarmaManager _karmaMan = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    // Target {Voter, vote}
    private Dictionary<NetUserId, Dictionary<NetUserId, int>> _storedVotes = new Dictionary<NetUserId, Dictionary<NetUserId, int>>();
    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<PlayerVoteEvent>(OnPlayerVote);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundEndCleanup);
    }

    private void OnPlayerVote(PlayerVoteEvent ev)
    {
        if (ev.Target == ev.Voter)
            return;

        if (!_storedVotes.TryGetValue(ev.Target, out var votes))
            _storedVotes[ev.Target] = new Dictionary<NetUserId, int>();
        _storedVotes[ev.Target][ev.Voter] = ev.Vote;

        // if (ev.VoterCreature != null)
        //     _popupSystem.PopupEntity($"{ev.Vote} Voted!", new EntityUid(ev.VoterCreature.Value), new EntityUid(ev.VoterCreature.Value), PopupType.Medium);
    }

    private void OnRoundEndCleanup(RoundRestartCleanupEvent ev)
    {
        foreach (var player in _storedVotes.Keys)
        {
            var howManyVotes = 0;
            foreach (var vote in _storedVotes.Values)
                foreach (var voter in vote.Keys)
                    if (voter == player)
                        howManyVotes++;

            var postiveVotes = 0;
            var negativeVotes = 0;
            var votes = _storedVotes[player];
            foreach (var vote in votes.Values)
            {
                if (vote > 0)
                    postiveVotes++;
                else if (vote < 0)
                    negativeVotes++;
            }

            var karmaChange = 0;
            if (negativeVotes == 0)
                karmaChange = (int) (10 + 5 * Math.Pow(1.5, postiveVotes));
            else
                karmaChange = (int) (5 * postiveVotes - 10 * Math.Pow(1.5, negativeVotes));
            karmaChange += Math.Min(howManyVotes, 5);

            _adminLogger.Add(LogType.Karma,
            LogImpact.Medium,
            $"Voting lost/gained Player {player}: {karmaChange} karma, from {postiveVotes} postives and {negativeVotes} negative votes, and from voting {howManyVotes} times");
            _karmaMan.AddKarma(player, karmaChange);
        }
        _storedVotes = new Dictionary<NetUserId, Dictionary<NetUserId, int>>();
    }
}
