using Content.Server.Body.Components;
using Content.Server.Body.Events;
using Content.Server.Traits.Assorted;
using Content.Shared.FixedPoint;

namespace Content.Server.Traits;

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

        var capped = FixedPoint2.Min(args.Amount, FixedPoint2.Zero);

        // Do the multiplication in doubles (avoids FixedPoint operator overloads)
        double bloodMax = (double) bloodstream.BloodMaxVolume; // FixedPoint2 has explicit operator double
        double percent = (double) ent.Comp.BloodLossPercentage; // works if float/double/FixedPoint2
        double lossDouble = bloodMax * percent;

        var lossFp = FixedPoint2.New(lossDouble);

        // Subtract by working with the internal 'cents' integer to avoid FixedPoint operator '-'
        args.Amount = FixedPoint2.FromCents(capped.Value - lossFp.Value);
    }
}
