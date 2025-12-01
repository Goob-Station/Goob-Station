using Content.Goobstation.Shared.Disease.Components;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Shared.Disease.Systems;

public sealed partial class DiseaseOnHitSystem : EntitySystem
{
    [Dependency] private readonly SharedDiseaseSystem _disease = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DiseaseOnHitComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(EntityUid uid, DiseaseOnHitComponent diseaseOnHit, ref MeleeHitEvent args)
    {
        if (!args.IsHit)
            return;

        foreach (var target in args.HitEntities)
        {
            if (diseaseOnHit.Disease != null)
            {
                _disease.DoInfectionAttempt(target, diseaseOnHit.Disease.Value, diseaseOnHit.SpreadParams);
            }
            else
            {
                if (!TryComp<DiseaseCarrierComponent>(uid, out var carrier))
                    return;

                foreach (var disease in carrier.Diseases)
                {
                    _disease.DoInfectionAttempt(target, disease, diseaseOnHit.SpreadParams);
                }
            }
        }
    }
}
