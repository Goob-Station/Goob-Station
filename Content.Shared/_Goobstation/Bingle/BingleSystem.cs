using Content.Shared.Weapons.Melee;
using Content.Shared.Interaction.Events;

namespace Content.Shared._Goobstation.Bingle;

public sealed class BingleSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BingleComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<BingleComponent, AttackAttemptEvent>(OnAttackAttempt);
    }
    private void OnMapInit(EntityUid uid, BingleComponent component, MapInitEvent args){
        if (component.Prime){
            var cords = Transform(uid).Coordinates;
            if (!(cords.X == 0 && cords.Y == 0)) //this whole looks a little clunky. have problems making sure it happesn only happens on spawn.
                Spawn("BinglePit", cords);
        }
    }
    //ran by the pit to upgrade bingle damage
    public void UpgradeBingle(EntityUid uid, BingleComponent component)
    {
        if (component.Upgraded)
            return; //end if already upgraded
        if (!TryComp<MeleeWeaponComponent>(uid, out var weponComp))
            return; //end if wepon comp not found

        weponComp.Damage = component.UpgradeDamage;
        component.Upgraded = true;
    }
    //Prevent Friendly Bingle fire
    private void OnAttackAttempt(EntityUid uid, BingleComponent component, AttackAttemptEvent args)
    {
        if (args.Cancelled)
            return;
        if (!(TryComp<BinglePitComponent>(args.Target, out var _) || TryComp<BingleComponent>(args.Target, out var _)))
            return;

        args.Cancel();
    }

}
