using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Fluids.Components;
using Content.Shared.Heretic.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server.Heretic.Ritual;

public sealed partial class RitualReagentPuddleBehavior : RitualCustomBehavior
{
    protected EntityLookupSystem _lookup = default!;

    [DataField] public ProtoId<ReagentPrototype>? Reagent;

    private List<EntityUid> uids = new();

    public override bool Execute(RitualData args, out string? outstr)
    {
        outstr = null;

        if (Reagent == null)
            return true;

        _lookup = args.EntityManager.System<EntityLookupSystem>();

        var lookup = _lookup.GetEntitiesInRange(args.Platform, .75f);

        foreach (var ent in lookup)
        {
            if (!args.EntityManager.HasComponent<PuddleComponent>(ent)
            || !args.EntityManager.TryGetComponent<SolutionContainerManagerComponent>(ent, out var solnMan))
                continue;

            if (solnMan.Solutions == null)
                continue;

            var sols = solnMan.Solutions;

            if (!sols["puddle"].ContainsPrototype(Reagent))
                continue;

            uids.Add(ent);
        }

        return true;
    }

    public override void Finalize(RitualData args)
    {
        foreach (var uid in uids)
            args.EntityManager.QueueDeleteEntity(uid);
    }
}
