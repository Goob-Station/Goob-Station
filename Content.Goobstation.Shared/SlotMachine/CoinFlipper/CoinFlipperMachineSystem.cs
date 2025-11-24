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

namespace Content.Goobstation.Shared.SlotMachine.CoinFlipper;

/// <summary>
/// This handles the coinflipper machine logic
/// </summary>
public sealed class CoinFlipperMachineSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedChatSystem _chatSystem = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
    [Dependency] private readonly SharedStackSystem _stackSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CoinFliperComponent, ActivateInWorldEvent>(OnInteractHandEvent);
        SubscribeLocalEvent<CoinFliperComponent, CoinFlipperDoAfterEvent>(OnSlotMachineDoAfter);
    }
    private void OnInteractHandEvent(EntityUid uid, CoinFliperComponent comp, ActivateInWorldEvent args)
    {
        if (comp.IsSpinning || !_power.IsPowered(uid))
            return;

        if (!_itemSlots.TryGetSlot(uid, "money", out var slot)
            || slot.Item == null
            || !TryComp<StackComponent>(slot.Item.Value, out var stack))
        {
            _popupSystem.PopupPredicted(Loc.GetString("slotmachine-no-money"), uid, uid, PopupType.Small); // No Money
            return;
        }

        comp.PrizeAmount = 0; //Reset prize amount just incase
        var doAfter =
         new DoAfterArgs(EntityManager, uid, comp.DoAfterTime, new CoinFlipperDoAfterEvent(), uid)
         {
             BreakOnMove = false,
             BreakOnDamage = false,
             MultiplyDelay = false,
         };
        comp.PrizeAmount = _stackSystem.GetCount(stack.Owner);
        _stackSystem.SetCount(stack.Owner, 0, stack);
        Dirty(stack.Owner, stack);
        comp.IsSpinning = true;

        if (_net.IsServer)
        {
            _audio.PlayPvs(comp.SpinSound, uid);
            _doAfter.TryStartDoAfter(doAfter);
        }
    }

    private void OnSlotMachineDoAfter(EntityUid uid, CoinFliperComponent comp, CoinFlipperDoAfterEvent args)
    {
        if (args.Cancelled) // Almost no way for it to be canceled but just in case
        {
            comp.IsSpinning = false;
            Dirty(uid, comp);
            return;
        }

        if (args.Handled || !_itemSlots.TryGetSlot(uid, "money", out var slot))
            return;

        comp.IsSpinning = false;
        Dirty(uid, comp);

        StackComponent? stack = null;
        if (slot.Item != null)
            TryComp<StackComponent>(slot.Item.Value, out stack);

        if (_random.Prob(.5f))
        {
            _audio.PlayPredicted(comp.WinSound, uid, args.User);
            if (stack == null)
            {
                var coordinates = Transform(uid).Coordinates;
                var newStack = EntityManager.SpawnEntity("SpaceCash", coordinates);
                if (TryComp<StackComponent>(newStack, out var newStackComp))
                {
                    comp.PrizeAmount *= 2;
                    _stackSystem.SetCount(newStack, comp.PrizeAmount, newStackComp);
                    Dirty(newStack, newStackComp);
                }

                _chatSystem.TrySendInGameICMessage(uid, Loc.GetString("coinflipper-win", ("amount", comp.PrizeAmount)), InGameICChatType.Speak, hideChat: false, hideLog: true, checkRadioPrefix: false);
                return;
            }
        }

        _audio.PlayPredicted(comp.LoseSound, uid, args.User); // If nothing then lose
    }
}
