using Content.Shared.Chat;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DoAfter;
using Content.Shared.Emag.Systems;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Random.Helpers;
using Content.Shared.Stacks;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.SlotMachine.ClawGame;

/// <summary>
/// This handles the coinflipper machine logic
/// </summary>
public sealed class ClawMachineSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ClawMachineComponent, ActivateInWorldEvent>(OnInteractHandEvent);
        SubscribeLocalEvent<ClawMachineComponent, ClawGameDoAfterEvent>(OnSlotMachineDoAfter);
        SubscribeLocalEvent<ClawMachineComponent, GotEmaggedEvent>(OnEmagged);
    }

    private void OnEmagged(EntityUid uid, ClawMachineComponent comp, ref GotEmaggedEvent args)
    {
        if(comp.Emagged)
            return;

        args.Handled = true;
        comp.Emagged = true;

        comp.Rewards = comp.EvilRewards; //My name is nhoj nhoj and I am EVIL
    }
    private void OnInteractHandEvent(EntityUid uid, ClawMachineComponent comp, ActivateInWorldEvent args)
    {
        if (comp.IsSpinning || !_power.IsPowered(uid))
            return;

        var doAfter =
         new DoAfterArgs(EntityManager, args.User, comp.DoAfterTime, new ClawGameDoAfterEvent(), uid)
         {
             BreakOnMove = true,
             BreakOnDamage = true,
             MultiplyDelay = false,
         };
        comp.IsSpinning = true;

        if (_net.IsServer)
        {
            _audio.PlayPvs(comp.PlaySound, uid);
            _doAfter.TryStartDoAfter(doAfter);
        }
        if (TryComp<AppearanceComponent>(uid, out var appearance) && _net.IsServer)
        {
            _appearance.SetData(uid, ClawMachineVisuals.Spinning, true);
            _appearance.SetData(uid, ClawMachineVisuals.NormalSprite, false);
        }
    }

    private void OnSlotMachineDoAfter(EntityUid uid, ClawMachineComponent comp, ClawGameDoAfterEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (args.Cancelled)
        {
            var selfMsgFail = Loc.GetString("clawmachine-fail-self");
            var othersMsgFail = Loc.GetString("clawmachine-fail-other", ("user", args.User));
            comp.IsSpinning = false;
            _popupSystem.PopupPredicted(selfMsgFail, othersMsgFail, args.User, args.User, PopupType.Small);
            if (TryComp<AppearanceComponent>(uid, out var _) && _net.IsServer)
            {
                _appearance.SetData(uid, ClawMachineVisuals.Spinning, false);
                _appearance.SetData(uid, ClawMachineVisuals.NormalSprite, true);
            }
            Dirty(uid, comp);
            return;
        }

        if (TryComp<AppearanceComponent>(uid, out var _) && _net.IsServer)
        {
            _appearance.SetData(uid, ClawMachineVisuals.Spinning, false);
            _appearance.SetData(uid, ClawMachineVisuals.NormalSprite, true);
        }
        comp.IsSpinning = false;
        Dirty(uid, comp);
        if(!_net.IsServer)
            return;

        if (_random.Prob(comp.WinChance) && comp.Rewards != null)
        {
            _audio.PlayPvs(comp.WinSound, uid);

            var rewardToSpawn = _random.Pick(comp.Rewards);

            var coordinates = Transform(uid).Coordinates;
            EntityManager.SpawnEntity(rewardToSpawn, coordinates);

            return;
        }

        _popupSystem.PopupEntity(Loc.GetString("clawmachine-fail-generic"), uid);
        _audio.PlayPvs(comp.LoseSound, uid);
    }
}
