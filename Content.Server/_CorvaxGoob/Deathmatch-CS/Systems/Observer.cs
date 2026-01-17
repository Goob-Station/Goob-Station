using Content.Server._CorvaxGoob.Deathmatch_CS.Components;
using Content.Server.GameTicking;
using Content.Server.Ghost.Roles.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Systems;

namespace Content.Server._CorvaxGoob.Deathmatch_CS.Systems;

public sealed class CSObserver : EntitySystem
{
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly CSRuleSystem _csRuleSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FighterComponent, MindAddedMessage>(OnMindAdded);
    }

    private void OnMindAdded(EntityUid uid, FighterComponent component, MindAddedMessage args)
    {
        var query = EntityQueryEnumerator<CSRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uId, out _, out var gRuleC))
        {
            if (!_gameTicker.IsGameRuleActive(uId, gRuleC))
                continue;

            foreach (var session in _csRuleSystem.Sessions)
            {
                if (session.MapId == EntityManager.GetComponent<TransformComponent>(uid).MapID)
                {
                    if (!session.Players.Contains(uid))
                        session.Players.Add(uid);

                    var query2 = EntityQueryEnumerator<FighterComponent, GhostRoleComponent>();

                    int count = 0;

                    while (query2.MoveNext(out var guid, out _, out _))
                    {
                        if (EntityManager.TryGetComponent(guid, out MindContainerComponent? mindContC))
                        {
                            if (mindContC.Mind == null && _mobStateSystem.IsAlive(guid))
                                count++;
                        }
                    }
                    if (count == 0)
                        _csRuleSystem.CreateNewSession();

                    break;
                }
            }
        }
    }
}
