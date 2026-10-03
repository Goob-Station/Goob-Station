using Content.Goobstation.Shared.Mech.Components;
using Content.Shared.Mech.Components;
using Robust.Shared.Containers;
using Robust.Shared.Serialization.Manager;

namespace Content.Goobstation.Server.Mech.Systems;

public sealed class GiveMechPilotSystem : EntitySystem
{
    [Dependency] private readonly ISerializationManager _serialization = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GiveMechPilotComponent, EntInsertedIntoContainerMessage>(OnPilotInserted);
        SubscribeLocalEvent<GiveMechPilotComponent, EntRemovedFromContainerMessage>(OnPilotRemoved);
    }

    private void OnPilotInserted(Entity<GiveMechPilotComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (!TryComp<MechComponent>(ent, out var mech) || args.Container.ID != mech.PilotSlotId)
            return;

        ent.Comp.Added.Clear();
        foreach (var (name, entry) in ent.Comp.Components)
        {
            var reg = Factory.GetRegistration(name);
            if (HasComp(args.Entity, reg.Type))
                continue;

            var comp = Factory.GetComponent(reg);
            _serialization.CopyTo(entry.Component, ref comp, notNullableOverride: true);
            AddComp(args.Entity, comp);
            ent.Comp.Added.Add(name);
        }
    }

    private void OnPilotRemoved(Entity<GiveMechPilotComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (!TryComp<MechComponent>(ent, out var mech) || args.Container.ID != mech.PilotSlotId)
            return;

        if (!TerminatingOrDeleted(args.Entity))
            foreach (var name in ent.Comp.Added)
                RemComp(args.Entity, Factory.GetRegistration(name).Type);

        ent.Comp.Added.Clear();
    }
}
