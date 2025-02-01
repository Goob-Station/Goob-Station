using Content.Shared.Disease;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.GameObjects;

namespace Content.Shared.Disease;

public sealed partial class DiseaseOnHitSystem : EntitySystem
{
    [Dependency] private readonly DiseaseSystem _disease = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DiseaseOnHitComponent, MeleeHitEvent>(OnMeleeHit);
    }

    public void OnMeleeHit(EntityUid uid, DiseaseOnHitComponent diseaseOnHit, ref MeleeHitEvent args)
    {
        if (!args.IsHit)
            return;

        foreach (var target in args.HitEntities)
        {
            _disease.TryInfect(target, diseaseOnHit.Disease);
        }
    }
}
