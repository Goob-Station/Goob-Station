using Content.Server.Body.Components;
using Content.Server.Body.Events;
using Content.Pirate.Server.Traits.BloodDeficiency.Components;
using Content.Shared.FixedPoint;

namespace Content.Pirate.Server.Traits.BloodDeficiency.Systems;

public sealed class BloodDeficiencySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BloodDeficiencyComponent, NaturalBloodRegenerationAttemptEvent>(OnBloodRegen);
    }

    private void OnBloodRegen(Entity<BloodDeficiencyComponent> ent, ref NaturalBloodRegenerationAttemptEvent args)
    {
        if (!ent.Comp.Active || !TryComp<BloodstreamComponent>(ent.Owner, out var bloodstream))
            return;

        args.Amount = FixedPoint2.Min(args.Amount, 0) // If the blood regen amount already was negative, we keep it.
                      - bloodstream.BloodMaxVolume * ent.Comp.BloodLossPercentage;
    }
}
