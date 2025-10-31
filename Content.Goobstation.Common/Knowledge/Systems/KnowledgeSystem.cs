using Content.Goobstation.Common.Knowledge.Components;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Common.Knowledge.Systems;

/// <summary>
/// This handles all knowledge related entities.
/// </summary>
public sealed partial class KnowledgeSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    private EntityQuery<KnowledgeComponent> _knowledgeQuery;
    private EntityQuery<KnowledgeContainerComponent> _containerQuery;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<KnowledgeContainerComponent, ComponentInit>(OnKnowledgeContainerInit);
        SubscribeLocalEvent<KnowledgeContainerComponent, ComponentShutdown>(OnKnowledgeContainerShutdown);
        SubscribeLocalEvent<KnowledgeContainerComponent, EntInsertedIntoContainerMessage>(OnEntityInserted);
        SubscribeLocalEvent<KnowledgeContainerComponent, EntRemovedFromContainerMessage>(OnEntityRemoved);

        _knowledgeQuery = GetEntityQuery<KnowledgeComponent>();
        _containerQuery = GetEntityQuery<KnowledgeContainerComponent>();
    }

    private void OnKnowledgeContainerInit(Entity<KnowledgeContainerComponent> ent, ref ComponentInit args)
    {
        ent.Comp.KnowledgeContainer =
            _container.EnsureContainer<Container>(ent.Owner, KnowledgeContainerComponent.ContainerId);
        // We show the contents of the container to allow knowledge to have visible sprites. I mean, if you really need to show some big brains.
        ent.Comp.KnowledgeContainer.ShowContents = true;
    }

    private void OnKnowledgeContainerShutdown(Entity<KnowledgeContainerComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.KnowledgeContainer is { } container)
            _container.ShutdownContainer(container);
    }

    private void OnEntityInserted(Entity<KnowledgeContainerComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != KnowledgeContainerComponent.ContainerId)
            return;

        if (!_knowledgeQuery.TryComp(args.Entity, out var statusComp))
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
        if (args.Container.ID != KnowledgeContainerComponent.ContainerId)
            return;

        if (!_knowledgeQuery.TryComp(args.Entity, out var statusComp))
            return;

        var ev = new KnowledgeUnitRemovedEvent(ent);
        RaiseLocalEvent(args.Entity, ref ev);

        // Clear AppliedTo after events are handled so event handlers can use it.
        if (statusComp.AppliedTo == null)
            return;

        // Why not just delete it? Well, that might end up being best, but this
        // could theoretically allow for moving status effects from one entity
        // to another. That might be good to have for polymorphs or something.
        statusComp.AppliedTo = null;
        Dirty(args.Entity, statusComp);
    }
}
