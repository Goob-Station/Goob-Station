using Content.Shared.Examine;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Shared.AlmanacBlade;

public abstract class SharedAlmanacBladeSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AlmanacBladeComponent, GetMeleeDamageEvent>(OnGetMeleeDamage);
        SubscribeLocalEvent<AlmanacBladeComponent, ExaminedEvent>(OnExamined);
    }

    private void OnGetMeleeDamage(Entity<AlmanacBladeComponent> ent, ref GetMeleeDamageEvent args)
    {
        if (ent.Comp.LeapDay)
        {
            args.Damage *= 0f;
            return;
        }

        if (!ent.Comp.Afternoon)
            args.Damage += ent.Comp.ColdDamage;
    }

    private void OnExamined(Entity<AlmanacBladeComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.WednesdayBeforeChristmas)
            args.PushMarkup(Loc.GetString("almanac-blade-examine-does-not"));
        else if (ent.Comp.MondayBeforeGarfield)
            args.PushMarkup(Loc.GetString("almanac-blade-examine-abilities"));
    }
}
