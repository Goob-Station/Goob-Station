using Content.Goobstation.Common.Heretic;
using Content.Goobstation.Common.Religion;
using Content.Goobstation.Common.Singularity;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Examine;
using Content.Shared.Heretic;

namespace Content.Shared._Shitcode.Heretic.Systems;

public sealed class LabyrinthHandbookSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystemShared _examine = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LabyrinthHandbookComponent, BeforeHolosignUsedEvent>(OnBeforeHolosign);

        SubscribeLocalEvent<ContainmentFieldThrowEvent>(OnThrow);
    }

    private void OnThrow(ref ContainmentFieldThrowEvent args)
    {
        if (!HasComp<LabyrinthWallComponent>(args.Field))
            return;

        if (HereticOrGhoul(args.Entity))
        {
            args.Cancelled = true;
            return;
        }

        var ev = new BeforeCastTouchSpellEvent(args.Entity, false);
        RaiseLocalEvent(args.Entity, ev, true);
        args.Cancelled = ev.Cancelled;
    }

    private void OnBeforeHolosign(Entity<LabyrinthHandbookComponent> ent, ref BeforeHolosignUsedEvent args)
    {
        args.Handled = true;

        if (!HereticOrGhoul(args.User) || !_examine.InRangeUnOccluded(args.User, args.ClickLocation))
            args.Cancelled = true;
    }

    private bool HereticOrGhoul(EntityUid uid)
    {
        return HasComp<HereticComponent>(uid) || HasComp<GhoulComponent>(uid);
    }
}
