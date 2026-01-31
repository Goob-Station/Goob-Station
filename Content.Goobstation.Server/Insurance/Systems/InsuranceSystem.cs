using Content.Server.Spawners.Components;
using Content.Server.Spawners.EntitySystems;
using Content.Shared.Destructible;
using Content.Goobstation.Server.Insurance.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Insurance.Systems;

public sealed partial class InsuranceSystem : EntitySystem
{
    [Dependency] private readonly SpawnOnDespawnSystem _spawnOnDespawn = default!;

    private LinkedList<ScheduledDrop> _scheduledDrops = [];

    public override void Initialize()
    {
        SubscribeLocalEvent<InsuranceComponent, DestructionEventArgs>(OnDestruct);
    }

    public override void Update(float frameTime)
    {
        UpdateScheduledDrops(frameTime);
    }

    private void OnDestruct(Entity<InsuranceComponent> ent, ref DestructionEventArgs args)
    {
        ScheduleDrop(ent);
    }

    private void ScheduleDrop(Entity<InsuranceComponent> ent)
    {
        var drop = new ScheduledDrop()
        {
            Target = ent.Comp.Beneficiary,
            Prototypes = new(ent.Comp.Policy.ExtraCompensationItems ?? []),
            TimeLeft = ent.Comp.Policy.DropDelay,
        };

        if (ent.Comp.Policy.IncludeTarget && Prototype(ent) is { } proto)
            drop.Prototypes.Add(proto);

        if (ent.Comp.Policy.DropDelay == null)
            Drop(drop);
        else
            _scheduledDrops.AddFirst(drop);
    }

    private void UpdateScheduledDrops(float frameTime)
    {
        var node = _scheduledDrops.First;
        while (node != null)
        {
            var next = node.Next;
            node.ValueRef.TimeLeft -= frameTime;
            if (node.Value.TimeLeft < 0)
            {
                Drop(node.Value);
                _scheduledDrops.Remove(node);
            }
            node = next;
        }
    }

    private void Drop(ScheduledDrop drop)
    {
        if (!Exists(drop.Target))
            return;

        if (!TryComp(drop.Target, out TransformComponent? xform))
            return;

        var dropPod = Spawn("SpawnSupplyEmpty", xform.Coordinates);
        var spawnOnDespawn = EnsureComp<SpawnOnDespawnComponent>(dropPod);
        _spawnOnDespawn.SetPrototypes((dropPod, spawnOnDespawn), drop.Prototypes);
    }

    private struct ScheduledDrop
    {
        public required EntityUid Target;

        public required List<EntProtoId> Prototypes;

        public float? TimeLeft;
    }
}
