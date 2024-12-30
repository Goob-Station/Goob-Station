using  Content.Shared.Weapons.Melee;

namespace Content.Shared._Goobstation.Bingle;

public sealed class BingleSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BingleComponent, MapInitEvent>(OnMapInit);
    }
    private void OnMapInit(Entity<BingleComponent> ent, ref MapInitEvent args)
    {
        //on mapint. check pit if this bingle shud be upgraded
        // this shud only afect bingles that spawn after pit evolves
        //if(pit==evolved) Upgrade(ent.Owner, ent.Comp)
    }
    public void UpgradeBingle(EntityUid uid, BingleComponent component)
    {
        if (!component.Upgraded)
            return; //end if already upgraded
        if (!TryComp<MeleeWeaponComponent>(uid, out var weponComp))
            return; //end if wepon comp not found

        component.Upgraded = true;
    }
}
