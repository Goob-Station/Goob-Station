using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared.PlayerVoting;
[Serializable, NetSerializable]
public sealed partial class PlayerVoteEvent : BoundUserInterfaceMessage // triggers the logic
{
    public NetUserId Voter;
    public NetUserId Target;
    public int Vote;

    public PlayerVoteEvent(NetUserId voter, NetUserId target, int vote)
    {
        Voter = voter;
        Target = target;
        Vote = vote;
    }
}
