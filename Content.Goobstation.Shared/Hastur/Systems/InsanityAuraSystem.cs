using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Hastur.Components;
using Content.Goobstation.Shared.Hastur.Events;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Hastur.Systems;

public sealed class InsanityAuraSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedInteractionSystem _interact = default!;

    private readonly HashSet<Entity<MobStateComponent>> _mobCache = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<InsanityAuraComponent, InsanityAuraEvent>(OnAura);
    }

    private void OnAura(Entity<InsanityAuraComponent> ent, ref InsanityAuraEvent args)
    {
        if (args.Handled)
            return;

        var (uid, comp) = ent;

        // Get coordinates of the aura source
        var centerCoords = Transform(uid).Coordinates;

        // Clear reusable cache and get nearby mobs only
        _mobCache.Clear();
        _lookup.GetEntitiesInRange(centerCoords, comp.Range, _mobCache);

        foreach (var mob in _mobCache)
        {
            if (mob.Owner == uid)
                continue;

            // Must have unobstructed line of sight
            if (!_interact.InRangeUnobstructed(uid, mob.Owner, comp.Range))
                continue;

            TryInjectReagents(mob.Owner, comp.Reagents);

            if (comp.VoidSound != null)
                _audio.PlayPredicted(comp.VoidSound, mob.Owner, mob.Owner);

            _popup.PopupEntity(Loc.GetString("hastur-insanityaura-begin3"), mob.Owner, mob.Owner);
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
