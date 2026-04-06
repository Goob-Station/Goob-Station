using Content.Goobstation.Common.Wanted;
using Content.Shared.CriminalRecords.Systems;
using Content.Shared.Examine;
using Content.Shared.Security;
using Content.Shared.Security.Components;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Wanted;

public sealed class NotorietySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    private const int DangerousIconThreshold = 4;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CriminalStatusUpdatedEvent>(OnStatusUpdated);
        SubscribeLocalEvent<NotorietyComponent, ExaminedEvent>(OnExamined);
    }

    private void OnStatusUpdated(CriminalStatusUpdatedEvent args)
    {
        if (args.Status is not (SecurityStatus.Wanted or SecurityStatus.Dangerous or SecurityStatus.Perma))
            return;

        var comp = EnsureComp<NotorietyComponent>(args.Entity);

        if (comp.Level >= NotorietyComponent.MaxLevel)
            return;

        comp.Level++;
        comp.LastEscalationTime = _timing.CurTime;
        comp.TotalEscalations++;
        Dirty(args.Entity, comp);

        if (comp.Level >= DangerousIconThreshold
            && TryComp<CriminalRecordComponent>(args.Entity, out var crimRecord))
        {
            crimRecord.StatusIcon = "SecurityIconDangerous";
            Dirty(args.Entity, crimRecord);
        }
    }

    private void OnExamined(Entity<NotorietyComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.Level <= 0)
            return;

        args.PushMarkup(Loc.GetString("notoriety-examine-bounty",
            ("credits", ent.Comp.BountyAmount)));
    }

    public int GetLevel(EntityUid uid)
    {
        return TryComp<NotorietyComponent>(uid, out var comp) ? comp.Level : 0;
    }

    public int GetBounty(EntityUid uid)
    {
        return TryComp<NotorietyComponent>(uid, out var comp) ? comp.BountyAmount : 0;
    }
}