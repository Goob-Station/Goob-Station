using Content.Goobstation.Shared.Wraith.Components;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared._Shitmed.Medical.Surgery;
using Content.Shared.Damage.Systems;
using Content.Shared.Examine;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Timing;


namespace Content.Goobstation.Server.Wraith.Systems;

public sealed partial class CursedBloodSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;
    [Dependency] private readonly SharedStatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly BloodstreamSystem _blood = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CursedBloodComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<CursedBloodComponent, MapInitEvent>(OnMapInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<CursedBloodComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            // Small puke Timer
            if (curTime >= comp.NextTickPuke)
            {
                _popup.PopupEntity(Loc.GetString("You cough up some blood. Something is wrong..."), uid, uid);
                //TO DO: Make them puke a litle bit of blood
                if (TryComp<BloodstreamComponent>(uid, out var blood))
                {
                    _blood.TryModifyBloodLevel(uid, comp.BleedAmount, blood);
                    _blood.TryModifyBleedAmount(uid, blood.MaxBleedAmount, blood);
                }
                // Schedule next puke tick
                comp.NextTickPuke = curTime + comp.TimeTillPuke;
            }

            // Big puke Timer
            if (curTime >= comp.NextTickBigPuke)
            {
                if (!comp.BloodCurseFullBloom)
                {
                    comp.BloodCurseFullBloom = true;
                }
                _popup.PopupEntity(Loc.GetString("Blood splatters all over the floor! Nasty!"), uid);
                //TO DO: Make them puke a lot of blood
                if (TryComp<BloodstreamComponent>(uid, out var blood))
                {
                    _blood.TryModifyBleedAmount(uid, 50f, blood);
                    _blood.SpillAllSolutions(uid, blood);
                }
                // Schedule next drowsy tick
                comp.NextTickBigPuke = curTime + comp.TimeTillBigPuke;
            }
        }
    }
    private void OnExamined(Entity<CursedBloodComponent> ent, ref ExaminedEvent args)
    {
        if (HasComp<WraithComponent>(args.Examiner))
        {
            //Tells the wraith that the target is cursed, and if the curse has fully bloomed or not.
            var bloomText = ent.Comp.BloodCurseFullBloom
                ? "The curse of blood has fully bloomed."
                : "The curse of blood has yet to fully bloom.";

            args.PushMarkup($"[color=darkred]{bloomText}[/color]");
        }
    }

    private void OnMapInit(Entity<CursedBloodComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextTickPuke = _timing.CurTime + ent.Comp.TimeTillPuke;
        ent.Comp.NextTickBigPuke = _timing.CurTime + ent.Comp.TimeTillBigPuke;
    }
}
