using Content.Goobstation.Shared.StepTrap;
using Content.Goobstation.Shared.Terror.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.Components;

namespace Content.Goobstation.Shared.Terror.Systems;

/// <summary>
/// System for injecting a reagent when an entity steps on this tile.
/// </summary>

public sealed class InjectorTileSystem : EntitySystem
{
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<InjectorTileComponent, StepTrapTriggeredEvent>(OnTriggered);
    }

    private void OnTriggered(EntityUid uid, InjectorTileComponent comp, ref StepTrapTriggeredEvent ev)
    {
        if (!TryComp<BloodstreamComponent>(ev.Tripper, out var bloodstream))
            return;

        var solution = new Solution();
        solution.AddReagent(comp.ReagentId, comp.Amount);
        _bloodstream.TryAddToChemicals((ev.Tripper, bloodstream), solution);
    }
}
