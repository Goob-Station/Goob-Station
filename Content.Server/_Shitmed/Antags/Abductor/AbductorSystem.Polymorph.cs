using Content.Shared._Shitmed.Antags.Abductor;
using Content.Shared.Polymorph;

namespace Content.Server._Shitmed.Antags.Abductor;

public sealed partial class AbductorSystem
{
    public void InitializePolymorph()
    {
        SubscribeLocalEvent<AbductorComponent, PolymorphedEvent>(OnAbductorPolymorphed);
        SubscribeLocalEvent<AbductorScientistComponent, PolymorphedEvent>(OnScientistPolymorphed);
        SubscribeLocalEvent<AbductorVictimComponent, PolymorphedEvent>(OnVictimPolymorphed);
    }

    private void OnAbductorPolymorphed(Entity<AbductorComponent> ent, ref PolymorphedEvent args)
    {
        if (args.NewEntity == ent.Owner)
            return;

        EnsureComp<AbductorComponent>(args.NewEntity);
        if (_tags.HasTag(ent, Abductor))
            _tags.AddTag(args.NewEntity, Abductor);
    }

    private void OnScientistPolymorphed(Entity<AbductorScientistComponent> ent, ref PolymorphedEvent args)
    {
        if (args.NewEntity == ent.Owner)
            return;

        var scientist = EnsureComp<AbductorScientistComponent>(args.NewEntity);
        scientist.SpawnPosition = ent.Comp.SpawnPosition;
        scientist.Console = ent.Comp.Console;
        Dirty(args.NewEntity, scientist);
    }

    private void OnVictimPolymorphed(Entity<AbductorVictimComponent> ent, ref PolymorphedEvent args)
    {
        if (args.NewEntity == ent.Owner)
            return;

        if (!HasComp<AbductorVictimComponent>(args.NewEntity))
        {
            var victim = EnsureComp<AbductorVictimComponent>(args.NewEntity);
            victim.Position = ent.Comp.Position;
            victim.Implanted = ent.Comp.Implanted;
            victim.LastActivation = ent.Comp.LastActivation;
            Dirty(args.NewEntity, victim);
        }

        var oldNet = GetNetEntity(ent.Owner);
        var newNet = GetNetEntity(args.NewEntity);

        var consoles = EntityQueryEnumerator<AbductorConsoleComponent>();
        while (consoles.MoveNext(out var uid, out var console))
        {
            if (console.Target != oldNet)
                continue;

            console.Target = newNet;
            UpdateGui(console.Target, (uid, console));
        }

        var gizmos = EntityQueryEnumerator<AbductorGizmoComponent>();
        while (gizmos.MoveNext(out _, out var gizmo))
        {
            if (gizmo.Target == oldNet)
                gizmo.Target = newNet;
        }
    }
}
