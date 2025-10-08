using Content.Goobstation.Shared.Wraith.Components.Mobs;
using Content.Goobstation.Shared.Wraith.Events;

namespace Content.Goobstation.Server.Wraith.Systems;
public sealed partial class EatFilthSystem : EntitySystem
{
    [Dependency] private readonly DiseasedRatSystem _diseasedRat = default!;
    private readonly List<(EntityUid, string)> _pendingEvolutions = new();
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
        if (HasComp<MediumRatComponent>(uid))
        {
            // Medium -> Giant
            if (comp.FilthConsumed >= comp.GiantFilthThreshold)
            {
                _pendingEvolutions.Add((uid, "MobPlagueRatGiant"));
                RemCompDeferred<DiseasedRatComponent>(uid); //Remove the component since we don't need to handle evolving any longer. Let's hope it doesn't break the eat filth action :clueless:
            }
        }
        else
        {
            // Small -> Medium
            if (comp.FilthConsumed >= comp.MediumFilthThreshold)
                _pendingEvolutions.Add((uid, "MobPlagueRatMedium"));
        }
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var (uid, proto) in _pendingEvolutions)
        {
            _diseasedRat.Evolve(uid, proto);
        }

        _pendingEvolutions.Clear();
    }

}
