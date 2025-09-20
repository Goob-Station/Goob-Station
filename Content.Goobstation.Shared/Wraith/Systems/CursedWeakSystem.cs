using Content.Goobstation.Shared.Wraith.Components;
using Content.Shared.Bed.Sleep;
using Content.Shared.Damage.Systems;
using Content.Shared.Examine;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Robust.Shared.Random;
using Robust.Shared.Timing;


namespace Content.Goobstation.Shared.Wraith.Systems;

public sealed partial class CursedWeakSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CursedWeakComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<CursedWeakComponent, MapInitEvent>(OnMapInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<CursedWeakComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (curTime < comp.NextTick)
                continue;

            if (comp.StaminaDamageAmount < comp.StaminaDamageMax)
            {
                comp.StaminaDamageAmount += comp.StaminaDamageIncrease;
                comp.WeakCurseFullBloom = true;
            }

            if (comp.SleepTimeAmount < comp.SleepTimeMax)
                comp.SleepTimeAmount += comp.SleepTimeIncrease;

            var effects = new Action[]
            {
        () => _stamina.TakeOvertimeStaminaDamage(uid, comp.StaminaDamageAmount),
        () => _statusEffects.TryAddStatusEffect<ForcedSleepingComponent>(
                uid, comp.StatusEffectKey, comp.SleepTimeAmount, false)
            };

            //TO DO: Add reduced stamina regeneration
            _random.Pick(effects)();

            // Schedule the next tick
            comp.NextTick = curTime + comp.TimeTillIncrement;
        }
    }

    private void OnExamined(Entity<CursedWeakComponent> ent, ref ExaminedEvent args)
    {
        if (HasComp<WraithComponent>(args.Examiner))
        {
            args.PushMarkup(
                $"[color=mediumpurple]{Loc.GetString("wraith-cursed-weak", ("target", ent.Owner))}[/color]");
        }
    }

    private void OnMapInit(Entity<CursedWeakComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextTick = _timing.CurTime + ent.Comp.TimeTillIncrement;
        Dirty(ent);
    }
}
