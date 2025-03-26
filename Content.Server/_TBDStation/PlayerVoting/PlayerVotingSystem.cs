
using Content.Shared.GameTicking;
using Content.Shared.PlayerVoting;
using Robust.Shared.Network;


namespace Content.Server._TBDStation.ServerKarma;

public sealed class ServerPlayerVotingSystem : EntitySystem
{
    private Dictionary<NetUserId, Tuple<NetUserId, int>> _storedVotes = new Dictionary<NetUserId, Tuple<NetUserId, int>>();
    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<PlayerVoteEvent>(OnPlayerVote);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundEndCleanup);
    }

    private void OnPlayerVote(PlayerVoteEvent ev)
    {
        _storedVotes[ev.Voter] = new Tuple<NetUserId, int>(ev.Target, ev.Vote);
        // if (_storedVotes.TryGetValue(ev.Voter , out var proofOfAsshole))
        // {
        // }
    }
    private void OnRoundEndCleanup(RoundRestartCleanupEvent ev)
    {
        // TODO apply karma changes
    }
}
