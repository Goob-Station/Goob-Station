using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Robust.Shared.Physics.Events;

namespace Content.Shared.Heretic.Effects;

public sealed class NoClipSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NoClipComponent, PreventCollideEvent>(OnPreventCollide);
        SubscribeLocalEvent<NoClipComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(Entity<NoClipComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.ExamineLoc is { } loc)
            args.PushMarkup(Loc.GetString(loc, ("ent", Identity.Entity(ent, EntityManager))));
    }

    private void OnPreventCollide(Entity<NoClipComponent> ent, ref PreventCollideEvent args)
    {
        if (!args.OtherFixture.Hard)
            return;

        args.Cancelled = true;
    }
}
