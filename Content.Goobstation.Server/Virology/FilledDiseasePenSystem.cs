using Content.Goobstation.Shared.Disease.Components;
using Content.Goobstation.Shared.Disease.Systems;
using Content.Goobstation.Shared.Virology;

namespace Content.Goobstation.Server.Virology;

public sealed class FilledDiseasePenSystem : EntitySystem
{
    [Dependency] private readonly SharedDiseaseSystem _disease = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<DiseasePenComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<DiseasePenComponent> ent, ref MapInitEvent args)
    {
        //Impossible to happen but just to make sure it isn't from machine
        if (ent.Comp.DiseaseUid != null)
            return;

        //Make sure that only the specified disease pen can be filled with disease
        if (!TryComp<GrantDiseaseComponent>(ent, out var granter))
            return;

        var disease = _disease.MakeRandomDisease(granter.BaseDisease, granter.Complexity);

        if (disease == null)
            return;

        ent.Comp.DiseaseUid = disease.Value;

        if (!TryComp<DiseaseComponent>(disease.Value, out var comp))
            return;

        var genotype = comp.Genotype;
        ent.Comp.Genotype = genotype;

        //Delete after disease have been created
        RemComp<GrantDiseaseComponent>(ent.Owner);
    }
}
