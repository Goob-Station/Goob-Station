using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.EntityEffects.EffectConditions;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Stunnable;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chat;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Examine;
using Content.Shared.Nutrition.Components;
using Content.Shared.Popups;
using Content.Shared.StatusIcon.Components;
using JetBrains.FormatRipper.Elf;
using Robust.Shared.Audio.Systems;
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
    [Dependency] private readonly SharedAudioSystem _audio = default!;

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

            if (curTime >= comp.NextTickPopup)
            {
               switch (comp.NextLine)
                {
                    case 1:
                        _popup.PopupEntity(Loc.GetString("curse-death-1"), uid, uid);
                        _audio.PlayEntity(comp.CurseSound1, uid, uid);
                        break;
                    case 2:
                        _popup.PopupEntity(Loc.GetString("curse-death-2"), uid, uid);
                        break;
                    case 3:
                        _popup.PopupEntity(Loc.GetString("curse-death-3"), uid, uid);
                        _audio.PlayEntity(comp.CurseSound2, uid, uid);
                        break;
                    case 4:
                        _popup.PopupEntity(Loc.GetString("curse-death-4"), uid, uid);
                        break;
                    case 5:
                        _popup.PopupEntity(Loc.GetString("curse-death-5"), uid, uid);
                        break;
                    case 6:
                        _popup.PopupEntity(Loc.GetString("curse-death-6"), uid, uid, PopupType.LargeCaution);
                        _audio.PlayEntity(comp.CurseSound3, uid, uid);
                        break;
                }

                comp.NextLine++;
                comp.NextTickPopup = curTime + comp.TimeTillPopup;
            }
            // Damage timer
            if (curTime >= comp.NextTickDamage)
            {
                _chatSystem.TrySendInGameICMessage(uid, "screams", InGameICChatType.Emote, false);
                _damage.TryChangeDamage(uid, comp.DamageCurse, targetPart: TargetBodyPart.All);
                comp.NextTickDamage = curTime + comp.TimeTillDamage;
            }

            // Stun timer
            if (curTime >= comp.NextTickStun)
            {
                _stun.TryKnockdown(uid, comp.StunDuration, true);
                comp.NextTickStun = curTime + comp.TimeTillStun;
            }

            // Stamina damage timer
            if (curTime >= comp.NextTickStaminaDamage)
            {
                comp.StaminaDamageAmount += comp.StaminaDamageIncrease;
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
                        var split = _solutionContainer.SplitSolution(solution.Value, FixedPoint2.New(comp.BloodToSpill));

                        // Spill their blood.
                        _puddle.TrySpillAt(uid, split, out _);
                    }
                }

                comp.NextTickGore = curTime + comp.TimeTillGore;
            }

            if (curTime >= comp.NextTickGib)
            {
                SpawnAtPosition(comp.SmokeProto, Transform(uid).Coordinates);
                _bodySystem.GibBody(uid);
                RemCompDeferred<CursedDeathComponent>(uid);

                //TO DO: Increase amount of corpses abosrbed.
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
