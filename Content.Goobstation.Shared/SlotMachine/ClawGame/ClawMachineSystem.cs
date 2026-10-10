using Content.Shared.DoAfter;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Random.Helpers;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.SlotMachine.ClawGame;

/// <summary>
/// This handles the coinflipper machine logic
/// </summary>
public sealed class ClawMachineSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PrizeSystem _prize = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ClawMachineComponent, ActivateInWorldEvent>(OnInteractHandEvent);
        SubscribeLocalEvent<ClawMachineComponent, ClawGameDoAfterEvent>(OnSlotMachineDoAfter);
        SubscribeLocalEvent<ClawMachineComponent, GotEmaggedEvent>(OnEmagged);
    }

    private void SetSpinning(Entity<ClawMachineComponent> ent, bool spinning)
    {
        _appearance.SetData(ent.Owner, ClawMachineVisuals.Spinning, spinning);
        _appearance.SetData(ent.Owner, ClawMachineVisuals.NormalSprite, !spinning);
        ent.Comp.IsSpinning = spinning;
    }

    private void OnEmagged(Entity<ClawMachineComponent> ent, ref GotEmaggedEvent args)
    {
        if (HasComp<EmaggedComponent>(ent.Owner))
            return;

        EnsureComp<EmaggedComponent>(ent.Owner);

        args.Handled = true;

        ent.Comp.Prizes = ent.Comp.EvilPrizes; // My name is nhoj nhoj and I am EVIL
        Dirty(ent);
    }
    private void OnInteractHandEvent(Entity<ClawMachineComponent> ent, ref ActivateInWorldEvent args)
    {
        if (ent.Comp.IsSpinning || !_power.IsPowered(ent.Owner))
            return;

        var doAfter = new DoAfterArgs(
            EntityManager,
            args.User,
            ent.Comp.DoAfterTime,
            new ClawGameDoAfterEvent(),
            ent
        )
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            MultiplyDelay = false,
        };

        _audio.PlayPvs(ent.Comp.PlaySound, ent.Owner);
        _doAfter.TryStartDoAfter(doAfter);

        Dirty(ent);
    }

    private void OnSlotMachineDoAfter(Entity<ClawMachineComponent> ent, ref ClawGameDoAfterEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (args.Cancelled)
        {
            var selfMsgFail = Loc.GetString("clawmachine-fail-self");
            var othersMsgFail = Loc.GetString("clawmachine-fail-other", ("user", args.User));

            _popupSystem.PopupPredicted(selfMsgFail, othersMsgFail, args.User, args.User);

            SetSpinning(ent, false);
            Dirty(ent);

            return;
        }

        SetSpinning(ent, false);
        Dirty(ent);

        var random = SharedRandomExtensions.PredictedRandom(_timing, GetNetEntity(ent));

        _prize.HandlePrize(ent.Comp.Prizes, ent.Owner, random);
    }
}
