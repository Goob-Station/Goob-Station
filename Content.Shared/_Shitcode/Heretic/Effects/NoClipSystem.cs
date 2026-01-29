using Robust.Shared.Physics.Events;

namespace Content.Shared.Heretic.Effects;

public sealed class NoClipSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NoClipComponent, PreventCollideEvent>(OnPreventCollide);
    }

    private void OnPreventCollide(Entity<NoClipComponent> ent, ref PreventCollideEvent args)
    {
        if (!args.OtherFixture.Hard)
            return;

        args.Cancelled = true;
    }
}
