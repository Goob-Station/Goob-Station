using Content.Shared.Examine;
using Content.Shared.IdentityManagement.Components;

namespace Content.Shared.IdentityManagement;

public class IdentityBlockerExamineSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IdentityBlockerComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(Entity<IdentityBlockerComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("identity-blocker-examine", ("coverage", (int)ent.Comp.Coverage)));
    }
}
