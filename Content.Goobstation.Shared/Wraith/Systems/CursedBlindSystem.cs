using Content.Goobstation.Shared.Wraith.Components;
using Content.Shared.Examine;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.Popups;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Wraith.Systems;

public sealed partial class CursedBlindSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly BlindableSystem _blindable = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CursedBlindComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<CursedBlindComponent, MapInitEvent>(OnMapInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<CursedBlindComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            // Already blind, nothing else to do.
            if (comp.FullyBlind)
                continue;

            // Not time yet
            if (curTime < comp.NextTick)
                continue;

            // Increment blindness if under max
            if (comp.BlindnessStacks < comp.MaxStacks)
            {
                comp.BlindnessStacks++;
                comp.NextTick = curTime + comp.TimeTillIncrement;

                if (TryComp<BlindableComponent>(uid, out var blindable))
                {
                    _blindable.AdjustEyeDamage((uid, blindable), 1);
                    _popup.PopupClient("Your eyesight worsens...", uid, uid);
                }

                // Aplly full bloom upon hitting max stacks.
                if (comp.BlindnessStacks >= comp.MaxStacks)
                    comp.FullyBlind = true;

                Dirty(uid, comp);
            }
        }
    }

    private void OnExamined(Entity<CursedBlindComponent> ent, ref ExaminedEvent args)
    {
        if (HasComp<WraithComponent>(args.Examiner))
        {
            //Tells the wraith that the target is cursed
            args.PushMarkup(
                $"[color=gray]{Loc.GetString("wraith-cursed-blind", ("target", ent.Owner))}[/color]");
        }
    }

    private void OnMapInit(Entity<CursedBlindComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextTick = _timing.CurTime + ent.Comp.TimeTillIncrement;
        Dirty(ent);
    }
}
