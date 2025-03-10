using Content.Shared.Examine;

namespace Content.Shared._DV.Construction;

public sealed class UpgradedMachineSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UpgradedMachineComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<UpgradedMachineComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        args.PushMarkup(Loc.GetString(ent.Comp.Upgrade));
    }
}
