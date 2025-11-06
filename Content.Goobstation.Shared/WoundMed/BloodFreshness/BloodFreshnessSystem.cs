using Content.Shared.Body._Goobstation;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Goobstation.Shared.WoundMed.BloodFreshness;

public abstract class BleedFreshnessSystem : EntitySystem
{
    [Dependency] protected readonly SharedSolutionContainerSystem SolutionContainer = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UpdateBloodFreshnessEvent>(OnUpdateBloodFreshness);
    }

    /// <summary>
    /// [WOUNDMED] This system is responsible for updating the blood freshness.
    /// </summary>
    private void OnUpdateBloodFreshness(ref UpdateBloodFreshnessEvent args)
    {
        var tempSolution = args.TempSolution;

        foreach (var dna in tempSolution
            .SelectMany(r => r.Reagent.EnsureReagentData().OfType<DnaData>()))
        {
            dna.Freshness = _timing.CurTime;
        }
    }
}
