using Content.Server._DV.CosmicCult.Components;
using Content.Server.RoundEnd;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Cuffs.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Shared.Player;

namespace Content.Server._DV.CosmicCult;

public sealed partial class CosmicCultRuleSystem
{
    [SubscribeLocalEvent]
    private void OnRoundEndRequestAttempt(ref RoundEndRequestAttemptEvent args)
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var rule, out _))
        {
            if (!IsBlockingEvac(rule))
                continue;

            args.Block("cult-alert-block-shuttle", color: Color.FromHex("#4cabb3"));
            return;
        }
    }

    private bool IsBlockingEvac(CosmicCultRuleComponent rule)
    {
        if (TryComp<CosmicFinaleComponent>(rule.MonumentInGame, out var finale)
            && finale.CurrentState == FinaleState.ActiveFinale)
            return true;

        if (rule.CurrentTier < 3)
            return false;

        if (rule.EvacBlockEnd is { } end && _timing.CurTime >= end)
            return false;

        return ActiveCultistsOnStation();
    }

    private bool ActiveCultistsOnStation()
    {
        var projecting = new HashSet<EntityUid>();
        var astralQuery = EntityQueryEnumerator<CosmicAstralBodyComponent, ActorComponent>();
        while (astralQuery.MoveNext(out _, out var astral, out _))
        {
            projecting.Add(astral.OriginalBody);
        }

        var query = EntityQueryEnumerator<CosmicCultComponent, MobStateComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var mobState, out var xform))
        {
            if (mobState.CurrentState != MobState.Alive)
                continue;

            if (!HasComp<ActorComponent>(uid) && !projecting.Contains(uid))
                continue;

            if (TryComp<CuffableComponent>(uid, out var cuffable) && cuffable.CuffedHandCount > 0)
                continue;

            if (_station.GetStationInMap(xform.MapID) == null)
                continue;

            return true;
        }

        return false;
    }
}
