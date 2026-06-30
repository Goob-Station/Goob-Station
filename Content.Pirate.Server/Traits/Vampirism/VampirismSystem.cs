using System.Linq;
using Content.Server.Body.Components;
using Content.Pirate.Server.Traits.Vampirism.Components;
using Content.Pirate.Shared.Vampire.Components;
using Content.Server.Body.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Body.Prototypes;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Analyzers;
using Robust.Shared.Prototypes;

namespace Content.Pirate.Server.Traits.Vampirism;

[Access(typeof(MetabolizerComponent), Other = AccessPermissions.ReadWriteExecute)]
public sealed class VampirismSystem : EntitySystem
{
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly MetabolizerSystem _metabolizer = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<VampirismComponent, MapInitEvent>(OnInitVampire);
    }

    private void OnInitVampire(Entity<VampirismComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp<BodyComponent>(ent, out var bodyCheck)
            || !_body.TryGetBodyOrganEntityComps<StomachComponent>((ent, bodyCheck), out var stomachComps)
            || stomachComps.Count == 0)
            return;

        // Mark vampire blood with VampireToxin on the DnaComponent so future blood generation includes it
        if (TryComp<Content.Shared.Forensics.Components.DnaComponent>(ent, out var dnaComp))
            dnaComp.VampireToxin = true;

        // Mark existing blood solution with VampireToxin so metabolism effects won't apply
        MarkVampireBloodWithToxin(ent);

        EnsureComp<BloodSuckerComponent>(ent);

        SetMetabolizerTypes(ent, ent.Comp.MetabolizerPrototypes);
    }

    public void SetMetabolizerTypes(EntityUid uid, HashSet<ProtoId<MetabolizerTypePrototype>> metabolizerTypes)
    {
        if (metabolizerTypes == null)
            return;

        if (!TryComp<BodyComponent>(uid, out var body)
            || !_body.TryGetBodyOrganEntityComps<MetabolizerComponent>((uid, body), out var comps))
            return;

        foreach (var comp in comps)
        {
            if (!TryComp<StomachComponent>(comp.Comp2.Owner, out var stomach))
                continue;

            _metabolizer.SetMetabolizerTypes((comp.Comp2.Owner, comp.Comp1), metabolizerTypes);
        }
    }

    /// <summary>
    /// Marks the vampire's blood with VampireToxin flag so metabolism effects won't apply.
    /// </summary>
    private void MarkVampireBloodWithToxin(EntityUid vampire)
    {
        if (!TryComp<BloodstreamComponent>(vampire, out var bloodstream))
            return;

        // Get the blood solution
        if (!_solutionContainer.ResolveSolution(vampire, bloodstream.BloodSolutionName, ref bloodstream.BloodSolution, out var solution))
            return;

        // Mark all DNA data in the solution with VampireToxin
        foreach (var reagent in solution.Contents)
        {
            var dnaDataList = reagent.Reagent.EnsureReagentData().OfType<DnaData>().ToList();
            foreach (var dnaData in dnaDataList)
            {
                dnaData.VampireToxin = true;
            }
        }
    }
}
