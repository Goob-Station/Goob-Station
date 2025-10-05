using Content.Goobstation.Shared.Wraith.Components.Mobs;
using Content.Goobstation.Shared.Wraith.Events;

namespace Content.Goobstation.Server.Wraith.Systems;
public sealed partial class EatFilthSystem : EntitySystem
{
    [Dependency] private readonly DiseasedRatSystem _diseasedRat = default!;
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
        {
            diseased.FilthConsumed++;

            CheckEvolution(ent.Owner, diseased);
        }

        QueueDel(target);

        args.Handled = true;
    }

    private void CheckEvolution(EntityUid uid, DiseasedRatComponent comp)
    {
        if (comp.FilthConsumed >= comp.GiantFilthThreshold)
        {
            _diseasedRat.Evolve(uid, "MobPlagueRatGiant");
            RemCompDeferred<DiseasedRatComponent>(uid);
        }
        else if (comp.FilthConsumed >= comp.MediumFilthThreshold)
        {
            _diseasedRat.Evolve(uid, "MobPlagueRatMedium");
        }
    }

}
