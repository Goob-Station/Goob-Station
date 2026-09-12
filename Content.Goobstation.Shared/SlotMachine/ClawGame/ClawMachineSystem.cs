using Content.Shared.DoAfter;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Power.EntitySystems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.SlotMachine.ClawGame;

/// <summary>
/// This handles the coinflipper machine logic
/// </summary>
public sealed class ClawMachineSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly PrizeSystem _prize = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ClawMachineComponent, ActivateInWorldEvent>(OnInteractHandEvent);
        SubscribeLocalEvent<ClawMachineComponent, ClawGameDoAfterEvent>(OnSlotMachineDoAfter);
        SubscribeLocalEvent<ClawMachineComponent, GotEmaggedEvent>(OnEmagged);
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

        var doAfter =
         new DoAfterArgs(EntityManager, args.User, ent.Comp.DoAfterTime, new ClawGameDoAfterEvent(), ent.Owner)
         {
             BreakOnMove = true,
             BreakOnDamage = true,
             MultiplyDelay = false,
         };
        ent.Comp.IsSpinning = true;

        if (_net.IsServer)
        {
            _audio.PlayPvs(ent.Comp.PlaySound, ent.Owner);
            _doAfter.TryStartDoAfter(doAfter);
            _appearance.SetData(ent.Owner, ClawMachineVisuals.Spinning, true);
            _appearance.SetData(ent.Owner, ClawMachineVisuals.NormalSprite, false);
        }

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

            ent.Comp.IsSpinning = false;
            _popupSystem.PopupPredicted(selfMsgFail, othersMsgFail, args.User, args.User);

            _appearance.SetData(ent, ClawMachineVisuals.Spinning, false);
            _appearance.SetData(ent, ClawMachineVisuals.NormalSprite, true);

            Dirty(ent);
            return;
        }

        _appearance.SetData(ent.Owner, ClawMachineVisuals.Spinning, false);
        _appearance.SetData(ent.Owner, ClawMachineVisuals.NormalSprite, true);

        ent.Comp.IsSpinning = false;

        Dirty(ent);

        if (_net.IsServer) // I have no fucking idea why this misperdicts on this only, when I try it on the slot machine its fine
            _prize.HandlePrize(ent.Comp.Prizes, ent.Owner);
    }
}
