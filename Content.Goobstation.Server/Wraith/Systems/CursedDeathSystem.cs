using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Body.Systems;
using Content.Shared.Chat;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Examine;
using Content.Shared.Jittering;
using Content.Shared.Popups;
using Robust.Shared.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Wraith.Systems;

public sealed partial class CursedDeathSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly BloodstreamSystem _blood = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly PuddleSystem _puddle = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly SharedChatSystem _chatSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CursedDeathComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<CursedDeathComponent, MapInitEvent>(OnMapInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<CursedDeathComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            // Damage timer
            if (curTime >= comp.NextTickDamage)
            {
                _chatSystem.TrySendInGameICMessage(uid, "screams", InGameICChatType.Emote, false);
                _damage.TryChangeDamage(uid, comp.DamageCurse, targetPart: TargetBodyPart.All);
                _popup.PopupClient(Loc.GetString("Your whole body is being torn apart!"), uid, uid);
                comp.NextTickDamage = curTime + comp.TimeTillDamage;
            }

            // Stun timer
            if (curTime >= comp.NextTickStun)
            {
                _stun.TryKnockdown(uid, comp.StunDuration, true);
                _popup.PopupClient(Loc.GetString("Your muscles seize up!"), uid, uid);
                comp.NextTickStun = curTime + comp.TimeTillStun;
            }

            // Stamina damage timer
            if (curTime >= comp.NextTickStaminaDamage)
            {
                comp.StaminaDamageAmount += comp.StaminaDamageIncrease;
                _popup.PopupClient(Loc.GetString("Your body feels heavy..."), uid, uid);
                _stamina.TakeOvertimeStaminaDamage(uid, comp.StaminaDamageAmount);

                comp.NextTickStaminaDamage = curTime + comp.TimeTillStaminaDamage;
            }

            // Gore (blood puke) timer
            if (curTime >= comp.NextTickGore)
            {
                if (TryComp<BloodstreamComponent>(uid, out var blood))
                {
                    if (_solutionContainer.TryGetSolution(uid, blood.BloodSolutionName, out var solution))
                    {
                        // Split out a fixed amount, e.g. 30 units
                        var split = _solutionContainer.SplitSolution(solution.Value, FixedPoint2.New(30));

                        // Spill the split blood at their feet
                        _puddle.TrySpillAt(uid, split, out _);
                        _popup.PopupClient(Loc.GetString("The end is nigh!"), uid, uid);
                    }
                }

                comp.NextTickGore = curTime + comp.TimeTillGore;
            }

            if (curTime >= comp.NextTickGib)
            {
                _bodySystem.GibBody(uid);
                RemCompDeferred<CursedDeathComponent>(uid);
            }
        }
    }

    private void OnExamined(Entity<CursedDeathComponent> ent, ref ExaminedEvent args)
    {
        //Tells the wraith that the target is cursed.
        args.PushMarkup(
            $"[color=white]{Loc.GetString("wraith-cursed-death", ("target", ent.Owner))}[/color]");
    }

    private void OnMapInit(Entity<CursedDeathComponent> ent, ref MapInitEvent args)
    {
        var curTime = _timing.CurTime;
        ent.Comp.NextTickDamage = curTime + ent.Comp.TimeTillDamage;
        ent.Comp.NextTickStun = curTime + ent.Comp.TimeTillStun;
        ent.Comp.NextTickStaminaDamage = curTime + ent.Comp.TimeTillStaminaDamage;
        ent.Comp.NextTickGore = curTime + ent.Comp.TimeTillGore;
        ent.Comp.NextTickGib = curTime + ent.Comp.TimeTillGib;
    }
}
