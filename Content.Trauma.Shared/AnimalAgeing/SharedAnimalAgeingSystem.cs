using Content.Shared.Examine;
using Content.Trauma.Shared.AnimalAgeing.Events;

namespace Content.Trauma.Shared.AnimalAgeing;

/// <summary>
/// This handles all the logic behind the animal ageing system
/// </summary>
public sealed class SharedAnimalAgeingSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AnimalAgeingComponent, AddAgeToMobAttemptEvent>(OnAddAgeAttempt);
        SubscribeLocalEvent<AnimalAgeingComponent, AddAgeToMobEvent>(OnAddAge);

        SubscribeLocalEvent<AnimalAgeingComponent, ChangeMobAgeStateAttemptEvent>(OnChangeStateAttempt);
        SubscribeLocalEvent<AnimalAgeingComponent, ChangeMobAgeStateEvent>(OnChangeState);

        SubscribeLocalEvent<AnimalAgeingComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(Entity<AnimalAgeingComponent> ent, ref ExaminedEvent args)
    {
        switch (ent.Comp.CurrentAgeState)
        {
            case AnimalAgeState.Baby:
                args.PushMarkup(Loc.GetString("age-markup-baby"));
                break;

            case AnimalAgeState.Adult:
                args.PushMarkup(Loc.GetString("age-markup-adult"));
                break;

            case AnimalAgeState.Senior:
                args.PushMarkup(Loc.GetString("age-markup-senior"));
                break;
        }
    }

    private void OnChangeState(Entity<AnimalAgeingComponent> ent, ref ChangeMobAgeStateEvent args)
    {
        ent.Comp.CurrentAgeState = args.NewState;
    }

    private void OnChangeStateAttempt(Entity<AnimalAgeingComponent> ent, ref ChangeMobAgeStateAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        var ev = new ChangeMobAgeStateEvent(ent, args.NewState);
        RaiseLocalEvent(ent.Owner, ref ev);
    }

    private void OnAddAge(Entity<AnimalAgeingComponent> ent, ref AddAgeToMobEvent args)
    {
        ent.Comp.YearsOld += args.Years;

        var yearsOld = ent.Comp.YearsOld;

        var ev = new ChangeMobAgeStateAttemptEvent();
        ev.Mob = ent;

        if (yearsOld >= ent.Comp.AdultHoodYear && ent.Comp.CurrentAgeState == AnimalAgeState.Baby)
            ev.NewState = AnimalAgeState.Adult;

        if (yearsOld >= ent.Comp.SeniorHoodYear && ent.Comp.CurrentAgeState == AnimalAgeState.Adult)
            ev.NewState = AnimalAgeState.Senior;

        if (yearsOld >= ent.Comp.SeniorHoodYear && ent.Comp.CurrentAgeState == AnimalAgeState.Senior)
        {
            var deathev = new OldAgeDeathAttemptEvent(ent);
            RaiseLocalEvent(ent.Owner, ref deathev);
        }

        RaiseLocalEvent(ent.Owner, ref ev);
    }

    private void OnAddAgeAttempt(Entity<AnimalAgeingComponent> ent, ref AddAgeToMobAttemptEvent args)
    {
        // TODO add that age stopping shit

        var ev = new AddAgeToMobEvent(ent, args.Years);
        RaiseLocalEvent(ent.Owner, ref ev);
    }
}
