using Content.Goobstation.Shared.Wraith.Components.Mobs;
using Content.Goobstation.Shared.Wraith.Events;

namespace Content.Goobstation.Shared.Wraith.Minions.Plaguebinger;
public sealed class EatFilthSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EatFilthComponent, EatFilthEvent>(OnEat);
    }

    private void OnEat(Entity<EatFilthComponent> ent, ref EatFilthEvent args)
    {
        // TODO: Popup
        ent.Comp.FilthConsumed++;
        Dirty(ent);

        var ev = new AteFilthEvent(ent.Comp.FilthConsumed);
        RaiseLocalEvent(ent.Owner, ref ev);

        PredictedQueueDel(args.Target);

        args.Handled = true;
    }
}
