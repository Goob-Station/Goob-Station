using Content.Shared._Harmony.Morph;
using Content.Shared.Alert.Components;
using Content.Shared.Polymorph.Components;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Client.Morph;

/// <summary>
/// Handles setting the morphs biomass UI
/// </summary>
public sealed class MorphSystem : EntitySystem
{

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MorphComponent, GetGenericAlertCounterAmountEvent>(OnUpdateAlert);
        SubscribeLocalEvent<MorphComponent, AttemptMeleeEvent>(OnAtack);
    }


    private void OnUpdateAlert(Entity<MorphComponent> ent, ref GetGenericAlertCounterAmountEvent args)
    {
        if (args.Handled)
            return;

        if (ent.Comp.BiomassAlert != args.Alert)
            return;

        args.Amount = ent.Comp.Biomass.Int();
    }
    private void OnAtack(EntityUid uid, MorphComponent component, ref AttemptMeleeEvent args)
    {
        //abort atack if morphed
        if (HasComp<ChameleonDisguisedComponent>(uid))
            args.Cancelled = true;
    }

}
