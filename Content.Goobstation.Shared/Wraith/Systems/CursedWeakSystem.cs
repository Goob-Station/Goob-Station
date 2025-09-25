using Content.Goobstation.Shared.Wraith.Components;
using Content.Shared.Bed.Sleep;
using Content.Shared.Damage.Systems;
using Content.Shared.Examine;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Random;
using Robust.Shared.Timing;


namespace Content.Goobstation.Shared.Wraith.Systems;

public sealed partial class CursedWeakSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;
    [Dependency] private readonly Content.Shared.StatusEffectNew.StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

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
            // Stamina Damage Timer
            if (curTime >= comp.NextTickStamina)
            {
                if (comp.StaminaDamageAmount < comp.StaminaDamageMax)
                {
                    comp.StaminaDamageAmount += comp.StaminaDamageIncrease;
                }

                _popup.PopupClient(Loc.GetString("Your body feels heavy..."), uid, uid);
                _stamina.TakeOvertimeStaminaDamage(uid, comp.StaminaDamageAmount);

                // Schedule next stamina tick
                comp.NextTickStamina = curTime + comp.TimeTillIncrementStamina;
            }

            // Drowsiness Timer
            if (curTime >= comp.NextTickDrowsy)
            {
                if (comp.SleepTimeAmount < comp.SleepTimeMax)
                    comp.SleepTimeAmount += comp.SleepTimeIncrease;

                _popup.PopupClient(Loc.GetString("You feel drowsy. Maybe now is a good time for a nap..."), uid, uid);
                _statusEffects.TryAddStatusEffect(uid, comp.ForcedSleep, out _, comp.SleepTimeAmount);

                // Schedule next drowsy tick
                comp.NextTickDrowsy = curTime + comp.TimeTillIncrementDrowsy;
            }
        }
    }
    private void OnExamined(Entity<CursedWeakComponent> ent, ref ExaminedEvent args)
    {
        if (HasComp<WraithComponent>(args.Examiner))
        {
            //Tells the wraith that the target is cursed.
            args.PushMarkup(
                $"[color=yellow]{Loc.GetString("wraith-cursed-weak", ("target", ent.Owner))}[/color]");
        }
    }

    private void OnMapInit(Entity<CursedWeakComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextTickStamina = _timing.CurTime + ent.Comp.TimeTillIncrementStamina;
        ent.Comp.NextTickDrowsy = _timing.CurTime + ent.Comp.TimeTillIncrementDrowsy;
        Dirty(ent); // I probably don't need to dirty it since I'm not showing it, but maybe the bool is affected.
    }
}
