using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Hastur.Components;
using Content.Goobstation.Shared.Hastur.Events;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Hastur.Systems;

public sealed class InsanityAuraSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedInteractionSystem _interact = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<InsanityAuraComponent, InsanityAuraEvent>(OnAura);
    }

    private void OnAura(Entity<InsanityAuraComponent> ent, ref InsanityAuraEvent args)
    {
        if (args.Handled)
            return;

        var uid = ent.Owner;
        var comp = ent.Comp;

        // Get everyone in visible range
        var entities = _lookup.GetEntitiesInRange(uid, comp.Range);

        foreach (var target in entities)
        {
            if (target == uid)
                continue;

            // Must have unobstructed line of sight
            if (!_interact.InRangeUnobstructed(uid, target, comp.Range))
                continue;

            // Inject configured reagents
            TryInjectReagents(target, comp.Reagents);

            // Play sound for that target only (client-local)
            if (comp.VoidSound != null)
                _audio.PlayPredicted(comp.VoidSound, target, target);

            // Optional feedback
            _popup.PopupEntity(Loc.GetString("hastur-insanityaura-begin3"), target, target);
        }

        args.Handled = true;
    }

    private bool TryInjectReagents(EntityUid target, Dictionary<ProtoId<ReagentPrototype>, FixedPoint2> reagents)
    {
        var solution = new Solution();
        foreach (var reagent in reagents)
        {
            solution.AddReagent(reagent.Key, reagent.Value);
        }

        if (!_solution.TryGetInjectableSolution(target, out var targetSolution, out _))
            return false;

        return _solution.TryAddSolution(targetSolution.Value, solution);
    }
}
