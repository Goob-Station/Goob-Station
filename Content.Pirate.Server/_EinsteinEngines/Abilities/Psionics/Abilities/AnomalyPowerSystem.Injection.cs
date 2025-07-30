using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions.Events;
using Content.Shared.Chemistry.Components.SolutionManager;
using System.Linq;

namespace Content.Server.Abilities.Psionics;

public sealed partial class AnomalyPowerSystem
{
    private EntityQuery<InjectableSolutionComponent> _injectableQuery;
    private void DoInjectionAnomalyEffects(EntityUid uid, PsionicComponent component, AnomalyPowerActionEvent args, bool overcharged = false)
    {
        if (args.Injection is null)
            return;

        if (overcharged)
            InjectionSupercrit(uid, component, args);
        else InjectionPulse(uid, component, args);
    }

    private void InjectionSupercrit(EntityUid uid, PsionicComponent component, AnomalyPowerActionEvent args)
    {
        var injection = args.Injection!.Value;
        var injectRadius = injection.SuperCriticalInjectRadius * component.CurrentAmplification;
        var maxInject = injection.SuperCriticalSolutionInjection * component.CurrentDampening;

        if (!_solutionContainer.TryGetSolution(uid, injection.Solution, out _, out var sol))
            return;

        //We get all the entity in the radius into which the reagent will be injected.
        var xformQuery = GetEntityQuery<TransformComponent>();
        var xform = xformQuery.GetComponent(uid);
        var allEnts = _lookup.GetEntitiesInRange<InjectableSolutionComponent>(_xform.GetMapCoordinates(uid), injectRadius)
            .Select(x => x.Owner).ToList();

        //for each matching entity found
        foreach (var ent in allEnts)
        {
            if (!_solutionContainer.TryGetInjectableSolution(ent, out var injectable, out _)
                || !_injectableQuery.TryGetComponent(ent, out var injEnt)
                || !_solutionContainer.TryTransferSolution(injectable.Value, sol, maxInject))
                continue;

            //Spawn Effect
            var uidXform = Transform(ent);
            Spawn(injection.VisualEffectPrototype, uidXform.Coordinates);
        }
    }

    private void InjectionPulse(EntityUid uid, PsionicComponent component, AnomalyPowerActionEvent args)
    {
        var injection = args.Injection!.Value;
        var injectRadius = injection.InjectRadius * component.CurrentAmplification;
        var maxInject = injection.MaxSolutionInjection * component.CurrentDampening;

        if (!_solutionContainer.TryGetSolution(uid, injection.Solution, out _, out var sol))
            return;

        //We get all the entity in the radius into which the reagent will be injected.
        var xformQuery = GetEntityQuery<TransformComponent>();
        var xform = xformQuery.GetComponent(uid);
        var allEnts = _lookup.GetEntitiesInRange<InjectableSolutionComponent>(_xform.GetMapCoordinates(uid), injectRadius)
            .Select(x => x.Owner).ToList();

        //for each matching entity found
        foreach (var ent in allEnts)
        {
            if (!_solutionContainer.TryGetInjectableSolution(ent, out var injectable, out _)
                || !_injectableQuery.TryGetComponent(ent, out var injEnt)
                || !_solutionContainer.TryTransferSolution(injectable.Value, sol, maxInject))
                continue;

            //Spawn Effect
            var uidXform = Transform(ent);
            Spawn(injection.VisualEffectPrototype, uidXform.Coordinates);
        }
    }
}