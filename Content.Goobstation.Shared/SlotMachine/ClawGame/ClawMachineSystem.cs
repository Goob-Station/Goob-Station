using Content.Shared.Chat;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Power.EntitySystems;
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
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ClawMachineComponent, ActivateInWorldEvent>(OnInteractHandEvent);
        SubscribeLocalEvent<ClawMachineComponent, ClawGameDoAfterEvent>(OnSlotMachineDoAfter);
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
    }

    private void OnSlotMachineDoAfter(EntityUid uid, ClawMachineComponent comp, ClawGameDoAfterEvent args)
    {

        if (args.Cancelled)
        {
            var selfMsgFail = Loc.GetString("clawmachine-fail-self");
            var othersMsgFail = Loc.GetString("clawmachine-fail-other", ("user", args.User));
            comp.IsSpinning = false;
            _popupSystem.PopupPredicted(selfMsgFail, othersMsgFail, args.User, args.User, PopupType.Small);
            Dirty(uid, comp);
            return;
        }

        if (args.Handled)
            return;

        comp.IsSpinning = false;
        Dirty(uid, comp);

        if (_random.Prob(comp.WinChance) && comp.Rewards != null)
        {
            _audio.PlayPredicted(comp.WinSound, uid, args.User);
            var RewardToSpawn = _random.Pick(comp.Rewards);

            var coordinates = Transform(uid).Coordinates;
            EntityManager.SpawnEntity(RewardToSpawn, coordinates);

            return;
        }

        var selfMsgFailEnd = Loc.GetString("clawmachine-fail-self");
        var othersMsgFailEnd = Loc.GetString("clawmachine-fail-other", ("user", args.User));
        _popupSystem.PopupPredicted(selfMsgFailEnd, othersMsgFailEnd, args.User, args.User, PopupType.Small);
        _audio.PlayPredicted(comp.LoseSound, uid, args.User); // If nothing then lose
    }
}
