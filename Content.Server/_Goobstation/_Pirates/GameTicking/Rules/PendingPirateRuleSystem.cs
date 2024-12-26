using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Shared.Dataset;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Goobstation._Pirates.GameTicking.Rules;

public sealed partial class PendingPirateRuleSystem : GameRuleSystem<PendingPirateRuleComponent>
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IRobustRandom _rand = default!;
    [Dependency] private readonly IPrototypeManager _prot = default!;
    [Dependency] private readonly GameTicker _gt = default!;

    [ValidatePrototypeId<EntityPrototype>] private readonly EntProtoId _PirateSpawnRule = "PiratesSpawn";

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = QueryActiveRules();
        while (eqe.MoveNext(out var uid, out _, out var pending, out var gamerule))
        {
            pending.PirateSpawnTimer += frameTime;
            if (pending.PirateSpawnTimer >= pending.PirateSpawnTime)
            {
                // TODO remove ransom
                SendAnnouncement((uid, pending), AnnouncementType.Arrival);
                _gt.StartGameRule(_PirateSpawnRule);
            }
        }
    }

    protected override void Started(EntityUid uid, PendingPirateRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        // TODO add ransom
        SendAnnouncement((uid, component), AnnouncementType.Threat);
    }

    public void SendAnnouncement(Entity<PendingPirateRuleComponent> pprule, AnnouncementType atype)
    {
        var announcer = pprule.Comp.LocAnnouncer;

        if (pprule.Comp.LocAnnouncers != null)
            announcer = _rand.Pick(_prot.Index<DatasetPrototype>(pprule.Comp.LocAnnouncers).Values);

        var type = atype.ToString().ToLower();
        announcer = Loc.GetString($"pirates-announcer-{announcer}");
        var announcement = Loc.GetString($"pirates-announcement-{announcer}-{type}");

        _chat.DispatchGlobalAnnouncement(announcement, announcer, colorOverride: Color.Orange);
    }

    public EntityQueryEnumerator<ActiveGameRuleComponent, PendingPirateRuleComponent, GameRuleComponent> GetPendingRules()
        => QueryActiveRules();

    public enum AnnouncementType
    {
        // should match with the localization strings
        Threat, Arrival, Paid, Cancelled, NotEnough
    }
}
