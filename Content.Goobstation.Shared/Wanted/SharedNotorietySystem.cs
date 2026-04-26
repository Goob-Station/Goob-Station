using Content.Goobstation.Common.Wanted;
using Content.Shared.Examine;

namespace Content.Goobstation.Shared.Wanted;

public abstract class SharedNotorietySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NotorietyComponent, ExaminedEvent>(OnExamined);
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