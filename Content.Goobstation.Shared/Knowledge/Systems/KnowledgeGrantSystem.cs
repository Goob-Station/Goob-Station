using System.Linq;
using Content.Goobstation.Common.Knowledge.Systems;
using Content.Goobstation.Shared.Knowledge.Components;
using Content.Shared.Body.Systems;
using Content.Shared.DoAfter;
using Content.Shared.EntityTable;
using Content.Shared.Interaction.Events;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Knowledge.Systems;

/// <summary>
/// Handles granting knowledge through different components and ways.
/// </summary>
public sealed class KnowledgeGrantSystem : EntitySystem
{
    [Dependency] private readonly EntityTableSystem _table = default!;
    [Dependency] private readonly KnowledgeSystem _knowledge = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<KnowledgeGrantComponent, MapInitEvent>(OnKnowledgeGrantInit, after: [typeof(SharedBodySystem)]);

        SubscribeLocalEvent<KnowledgeGrantOnUseComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<KnowledgeGrantOnUseComponent, KnowledgeLearnDoAfterEvent>(OnDoAfter);
    }

    private void OnKnowledgeGrantInit(Entity<KnowledgeGrantComponent> ent, ref MapInitEvent args)
    {
        var units = _table.GetSpawns(ent.Comp.Table).ToList();
        _knowledge.AddKnowledgeUnits(ent.Owner, units);
        RemComp(ent.Owner, ent.Comp);
    }

    private void OnUseInHand(Entity<KnowledgeGrantOnUseComponent> ent, ref UseInHandEvent args)
    {
        var (uid, comp) = ent;

        if (comp.DoAfter is null)
        {
            var units = _table.GetSpawns(ent.Comp.Table).ToList();
            _knowledge.AddKnowledgeUnits(args.User, units);
        }
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

        var units = _table.GetSpawns(ent.Comp.Table).ToList();
        _knowledge.AddKnowledgeUnits(args.Target.Value, units);
    }
}

[Serializable, NetSerializable]
public sealed partial class KnowledgeLearnDoAfterEvent : SimpleDoAfterEvent;
