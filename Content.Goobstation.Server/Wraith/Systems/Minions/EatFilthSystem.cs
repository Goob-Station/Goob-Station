using Content.Goobstation.Shared.Wraith.Components.Mobs;
using Content.Goobstation.Shared.Wraith.Events;

namespace Content.Goobstation.Shared.Wraith.Systems;
public sealed partial class EatFilthSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EatFilthComponent, EatFilthEvent>(OnEat);
    }
    public void OnEat(Entity<EatFilthComponent> ent, ref EatFilthEvent args)
    {
        var uid = ent.Owner;
        var target = args.Target;
        var comp = ent.Comp;

        if (args.Handled)
            return;

        if (TryComp<DiseasedRatComponent>(ent, out var diseased))
            diseased.FilthConsumed++;

        QueueDel(target);

        args.Handled = true;
    }
}
