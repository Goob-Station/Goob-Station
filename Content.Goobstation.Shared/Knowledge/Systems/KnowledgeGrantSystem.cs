using Content.Goobstation.Common.Knowledge.Systems;
using Content.Goobstation.Shared.Knowledge.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Construction;
using Content.Shared.Construction.Components;
using Content.Shared.DoAfter;
using Content.Shared.Interaction.Events;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Knowledge.Systems;

/// <summary>
/// Handles granting knowledge through different components and ways.
/// </summary>
public sealed class KnowledgeGrantSystem : EntitySystem
{
    [Dependency] private readonly KnowledgeSystem _knowledge = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<KnowledgeGrantComponent, MapInitEvent>(OnKnowledgeGrantInit, after: [typeof(SharedBodySystem)]); // General component for general shitspawning
        SubscribeLocalEvent<ConstructionKnowledgeGrantComponent, MapInitEvent>(OnConstructionGrantInit, after: [typeof(SharedBodySystem)]); // For construction knowledge

        SubscribeLocalEvent<KnowledgeGrantOnUseComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<KnowledgeGrantOnUseComponent, KnowledgeLearnDoAfterEvent>(OnDoAfter);
    }

    private void OnKnowledgeGrantInit(Entity<KnowledgeGrantComponent> ent, ref MapInitEvent args)
    {
        _knowledge.AddKnowledgeUnits(ent.Owner, ent.Comp.ToAdd);
        RemComp(ent.Owner, ent.Comp);
    }

    private void OnUseInHand(Entity<KnowledgeGrantOnUseComponent> ent, ref UseInHandEvent args)
    {
        var (uid, comp) = ent;

        if (comp.DoAfter is null)
            _knowledge.AddKnowledgeUnits(args.User, comp.ToAdd);
        else
        {
            var doAfter = new DoAfterArgs(
                EntityManager,
                args.User,
                TimeSpan.FromSeconds(comp.DoAfter.Value),
                new KnowledgeLearnDoAfterEvent(),
                uid,
                uid,
                uid)
            {
                BreakOnDropItem = true,
                BreakOnHandChange = true,
                BreakOnDamage = true,
                BreakOnMove = true,
                BlockDuplicate = true,
            };

            _doAfter.TryStartDoAfter(doAfter);
        }
    }

    private void OnDoAfter(Entity<KnowledgeGrantOnUseComponent> ent, ref KnowledgeLearnDoAfterEvent args)
    {
        if (args.Handled
            || args.Cancelled
            || args.Target == null
            || TerminatingOrDeleted(args.Target))
            return;

        _knowledge.AddKnowledgeUnits(args.Target.Value, ent.Comp.ToAdd);
    }


    private void OnConstructionGrantInit(Entity<ConstructionKnowledgeGrantComponent> ent, ref MapInitEvent args)
    {
        if (!_knowledge.TryEnsureKnowledgeUnit(ent.Owner, SharedConstructionSystem.ConstructionKnowledge, out var knowledge)
            || !TryComp(knowledge, out ConstructionKnowledgeComponent? knowledgeComp))
            return;

        foreach (var group in ent.Comp.Groups)
        {
            knowledgeComp.Groups.Add(group);
        }

        Dirty(knowledge.Value, knowledgeComp);
        RemComp(ent.Owner, ent.Comp);
    }
}

[Serializable, NetSerializable]
public sealed partial class KnowledgeLearnDoAfterEvent : SimpleDoAfterEvent;
