using Content.Goobstation.Common.Knowledge;
using Content.Goobstation.Common.Knowledge.Components;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.Knowledge.Systems;

public sealed partial class KnowledgeSystem : CommonKnowledgeSystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;

    private EntityQuery<KnowledgeComponent> _knowledgeQuery;
    private EntityQuery<KnowledgeContainerComponent> _containerQuery;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<KnowledgeContainerComponent, ComponentShutdown>(OnKnowledgeContainerShutdown);
        SubscribeLocalEvent<KnowledgeContainerComponent, EntInsertedIntoContainerMessage>(OnEntityInserted);
        SubscribeLocalEvent<KnowledgeContainerComponent, EntRemovedFromContainerMessage>(OnEntityRemoved);

        _knowledgeQuery = GetEntityQuery<KnowledgeComponent>();
        _containerQuery = GetEntityQuery<KnowledgeContainerComponent>();
    }

    private void OnKnowledgeContainerShutdown(Entity<KnowledgeContainerComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.KnowledgeContainer is { } container)
            _container.ShutdownContainer(container);
    }

    private void OnEntityInserted(Entity<KnowledgeContainerComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != KnowledgeContainerComponent.ContainerId
            || !_knowledgeQuery.TryComp(args.Entity, out var statusComp))
            return;

        // Make sure AppliedTo is set correctly so events can rely on it
        if (statusComp.AppliedTo != ent)
        {
            statusComp.AppliedTo = ent;
            Dirty(args.Entity, statusComp);
        }

        var ev = new KnowledgeUnitAddedEvent(ent);
        RaiseLocalEvent(args.Entity, ref ev);
    }

    private void OnEntityRemoved(Entity<KnowledgeContainerComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != KnowledgeContainerComponent.ContainerId
            || !_knowledgeQuery.TryComp(args.Entity, out var statusComp))
            return;

        var ev = new KnowledgeUnitRemovedEvent(ent);
        RaiseLocalEvent(args.Entity, ref ev);

        // Clear AppliedTo after events are handled so event handlers can use it.
        if (statusComp.AppliedTo == null)
            return;

        // Why not just delete it? Well, that might allow to transfer this knowledge to other entities,
        // for example as a wizard spell to steal all knowledge from someone.
        statusComp.AppliedTo = null;
        Dirty(args.Entity, statusComp);
    }
}
