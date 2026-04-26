using Content.Goobstation.Common.Wanted;
using Content.Goobstation.Shared.Wanted;
using Content.Shared.Security;
using Content.Shared.Security.Components;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Wanted;

public sealed class NotorietySystem : SharedNotorietySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    private const int DangerousIconThreshold = 4;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CriminalStatusUpdatedEvent>(OnStatusUpdated);
    }

    private void OnStatusUpdated(CriminalStatusUpdatedEvent args)
    {
        var status = (SecurityStatus) args.Status;
        if (status is not (SecurityStatus.Wanted or SecurityStatus.Dangerous or SecurityStatus.Perma))
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
}