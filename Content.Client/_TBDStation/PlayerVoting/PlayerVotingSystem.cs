using Content.Client.Pointing.Components;
using Content.Shared.Pointing;
using Content.Shared.Verbs;
using Robust.Client.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;
using DrawDepth = Content.Shared.DrawDepth.DrawDepth;
using Robust.Shared.GameObjects;
using Content.Shared.PlayerVoting;
using Robust.Shared.Player;
using Robust.Shared.Network;

namespace Content.Client._TBDStation.PlayerVoting;

public sealed partial class PlayerVotingSystem : EntitySystem
{
    [Dependency] private readonly ActorSystem _actors = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GetVerbsEvent<Verb>>(AddVotingVerb);
    }

    private void AddVotingVerb(GetVerbsEvent<Verb> args)
    {
        if (IsClientSide(args.Target))
            return;

        var voter = GetNetIdFromEntId(args.User);
        var target = GetNetIdFromEntId(args.Target);
        if (voter == null || target == null)
            return;

        Verb verb = new()
        {
            Text = "Vote",
            Icon = new SpriteSpecifier.Texture(new ("/Textures/Interface/VerbIcons/point.svg.192dpi.png")),
            ClientExclusive = true,
            Act = () => RaiseNetworkEvent(new PlayerVoteEvent(voter.Value, target.Value, 1))
        };

        args.Verbs.Add(verb);
    }

    private NetUserId? GetNetIdFromEntId(EntityUid entity)
    {
        if (!_actors.TryGetSession(entity, out ICommonSession? session))
            return null;
        if (session == null)
            return null;
        return session.UserId;
    }

}
