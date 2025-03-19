using Content.Server._Goobstation._Pirates.GameTicking.Rules;
using Content.Server.GameTicking;
using Content.Shared.GameTicking.Components;

namespace Content.Server._Goobstation._Pirates.Pirates;

public sealed partial class RansomSystem : EntitySystem
{
    [Dependency] private readonly PendingPirateRuleSystem _pprs = default!;
    [Dependency] private readonly GameTicker _gt = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RansomComponent, ComponentStartup>(OnGetRansom);
    }

    private void OnGetRansom(Entity<RansomComponent> ent, ref ComponentStartup args)
    {
        var eqe = EntityQueryEnumerator<PendingPirateRuleComponent, GameRuleComponent>();
        while (eqe.MoveNext(out var uid, out var prule, out var gamerule))
        {
            _gt.EndGameRule(uid, gamerule);
            _pprs.SendAnnouncement((uid, prule), PendingPirateRuleSystem.AnnouncementType.Paid);
        }
    }
}
