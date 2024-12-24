using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Rules;
using Content.Shared.GameTicking.Components;

namespace Content.Server._Goobstation._Pirates.GameTicking.Rules;

public sealed partial class PendingPirateRuleSystem : GameRuleSystem<PendingPirateRuleComponent>
{
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = QueryActiveRules();
        while (eqe.MoveNext(out _, out _, out var pending, out var gamerule))
        {
            pending.PirateSpawnTimer += frameTime;
            if (pending.PirateSpawnTimer >= pending.PirateSpawnTime)
            {
                // remove ransom
                // add pirates
            }
        }
    }

    protected override void Started(EntityUid uid, PendingPirateRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var announcer = component.LocAnnouncer;
        var announcement = component.LocAnnouncement;

        if (component.LocAnnouncers != null)
        {

        }
        if (component.LocAnnouncements != null)
        {

        }

        announcer = Loc.GetString($"pirates-announcer-{announcer}");
        announcement = Loc.GetString($"");

        _chat.DispatchGlobalAnnouncement(announcement, announcer, colorOverride: Color.Orange);
    }

    private void TimeThresholdReached()
    {

    }
    private void Paid()
    {

    }
    private void DidntPayEnough()
    {

    }

    public EntityQueryEnumerator<ActiveGameRuleComponent, PendingPirateRuleComponent, GameRuleComponent> GetPendingRules()
        => QueryActiveRules();
}
