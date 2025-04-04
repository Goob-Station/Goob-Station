using Content.Shared.Examine;

namespace Content.Goobstation.Shared.Wizard.ArcaneBarrage;

public sealed class DeleteOnDropAttemptSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<Common.Wizard.ArcaneBarrage.DeleteOnDropAttemptComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(Entity<Common.Wizard.ArcaneBarrage.DeleteOnDropAttemptComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("delete-on-drop-attempt-comp-examine"));
    }
}
