using System.Linq;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Events;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Rejuvenate;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Timing;

namespace Content.Shared._Shitcode.Heretic.Systems;

public sealed class EldritchSleepStatusEffectSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _sol = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EldritchSleepStatusEffectComponent, StatusEffectAppliedEvent>(OnApply);
        SubscribeLocalEvent<EldritchSleepStatusEffectComponent, StatusEffectRemovedEvent>(OnRemove);

        SubscribeLocalEvent<MetabolismModifierComponent, GetMetabolicMultiplierEvent>(OnGetMultiplier);
    }

    private void OnGetMultiplier(Entity<MetabolismModifierComponent> ent, ref GetMetabolicMultiplierEvent args)
    {
        args.Multiplier *= ent.Comp.Modifier;
    }

    private void OnRemove(Entity<EldritchSleepStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (_timing.ApplyingState)
            return;

        EntityManager.RemoveComponents(args.Target, ent.Comp.ComponentDifference);
        if (!TryComp(args.Target, out BloodstreamComponent? bloodstream) || !_sol.ResolveSolution(args.Target,
                bloodstream.ChemicalSolutionName,
                ref bloodstream.ChemicalSolution,
                out var sol))
            return;

        sol.RemoveAllSolution();
    }

    private void OnApply(Entity<EldritchSleepStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        if (_timing.ApplyingState)
            return;

        var ev = new RejuvenateEvent(false, false);
        RaiseLocalEvent(args.Target, ev);

        var difference =
            ent.Comp.ComponentsToAdd.ExceptBy(EntityManager.GetComponents(args.Target), x => x.Value.Component)
                .ToDictionary();

        ent.Comp.ComponentDifference = new(difference);
        EntityManager.AddComponents(args.Target, ent.Comp.ComponentsToAdd);
    }
}
