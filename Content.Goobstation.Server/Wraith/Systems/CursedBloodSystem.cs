using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Wraith.Components;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared._Shitmed.Medical.Surgery;
using Content.Shared.Body.Systems;
using Content.Shared.Chat;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage.Systems;
using Content.Shared.Examine;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Timing;
using Content.Server.Fluids.EntitySystems;
using Content.Shared.Body.Components;


namespace Content.Goobstation.Server.Wraith.Systems;

public sealed partial class CursedBloodSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly BloodstreamSystem _blood = default!;
    [Dependency] private readonly SharedChatSystem _chatSystem = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly PuddleSystem _puddle = default!;

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
                _chatSystem.TrySendInGameICMessage(uid, "coughs", InGameICChatType.Emote, false);
                //Make them puke a litle bit of blood
                if (TryComp<BloodstreamComponent>(uid, out var blood))
                {
                    _blood.TryModifyBloodLevel((uid, blood), comp.BleedAmount);
                    _blood.TryModifyBleedAmount((uid, blood), blood.MaxBleedAmount);
                }
                // Schedule next puke tick
                comp.NextTickPuke = curTime + comp.TimeTillPuke;
            }

            // Big puke Timer
            if (curTime >= comp.NextTickBigPuke)
            {
                _popup.PopupEntity(Loc.GetString("Blood splatters all over the floor! Nasty!"), uid);
                //Make them puke a lot of blood
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
                // Schedule next drowsy tick
                comp.NextTickBigPuke = curTime + comp.TimeTillBigPuke;
            }
        }
    }
    private void OnExamined(Entity<CursedBloodComponent> ent, ref ExaminedEvent args)
    {
        if (HasComp<WraithComponent>(args.Examiner))
            //Tells the wraith that the target is cursed, and if the curse has fully bloomed or not.
            args.PushMarkup($"[color=darkred]{Loc.GetString("wraith-cursed-blood", ("target", ent.Owner))}[/color]");
    }
    private void OnMapInit(Entity<CursedBloodComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextTickPuke = _timing.CurTime + ent.Comp.TimeTillPuke;
        ent.Comp.NextTickBigPuke = _timing.CurTime + ent.Comp.TimeTillBigPuke;
    }
}
