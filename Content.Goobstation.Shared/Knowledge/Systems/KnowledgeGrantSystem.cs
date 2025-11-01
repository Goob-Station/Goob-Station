using Content.Goobstation.Common.Knowledge.Systems;
using Content.Goobstation.Shared.Knowledge.Components;
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

        SubscribeLocalEvent<KnowledgeGrantComponent, MapInitEvent>(OnKnowledgeGrantInit);

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
}

[Serializable, NetSerializable]
public sealed partial class KnowledgeLearnDoAfterEvent : SimpleDoAfterEvent;
