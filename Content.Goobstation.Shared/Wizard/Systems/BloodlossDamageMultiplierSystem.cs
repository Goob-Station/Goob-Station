using Content.Goobstation.Common.Bloodstream;
using Content.Goobstation.Shared.Wizard.Components;

namespace Content.Goobstation.Shared.Wizard.Systems;

public sealed partial class BloodlossDamageMultiplierSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodlossDamageMultiplierComponent, StoppedTakingBloodlossDamageEvent>(OnBloodlossStopped);
        SubscribeLocalEvent<BloodlossDamageMultiplierComponent, GetBloodlossDamageMultiplierEvent>(OnGetBloodlossMultiplier);
    }

    private void OnGetBloodlossMultiplier(Entity<BloodlossDamageMultiplierComponent> ent,
        ref GetBloodlossDamageMultiplierEvent args)
    {
        args.Multiplier *= ent.Comp.Multiplier;
    }
    private void OnBloodlossStopped(Entity<BloodlossDamageMultiplierComponent> ent,
        ref StoppedTakingBloodlossDamageEvent args)
    {
        RemCompDeferred(ent.Owner, ent.Comp);
    }
}