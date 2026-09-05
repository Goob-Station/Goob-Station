using Content.Shared.Chat;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Coordinates;
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

    private void OnInteractHandEvent(Entity<CoinFliperComponent> ent, ref ActivateInWorldEvent args)
    {
        if (ent.Comp.IsSpinning || !_power.IsPowered(ent.Owner))
            return;

        if (!_itemSlots.TryGetSlot(ent.Owner, "money", out var slot)
            || slot.Item is not { } item)
        {
            _popupSystem.PopupPredicted(Loc.GetString("slotmachine-no-money"), ent.Owner, args.User); // No Money
            return;
        }

        ent.Comp.PrizeAmount = 0; //Reset prize amount just incase
        var doAfter =
         new DoAfterArgs(EntityManager, ent.Owner, ent.Comp.DoAfterTime, new CoinFlipperDoAfterEvent(), ent.Owner)
         {
             BreakOnMove = false,
             BreakOnDamage = false,
             MultiplyDelay = false,
         };

        if (TryComp<StackComponent>(item, out var stack))
        {
            ent.Comp.PrizeAmount = _stackSystem.GetCount(item);
            PredictedDel(item);
        }

        ent.Comp.IsSpinning = true;

        if (_net.IsServer)
        {
            _audio.PlayPvs(ent.Comp.SpinSound, ent.Owner);
            _doAfter.TryStartDoAfter(doAfter);
        }
    }

    private void OnSlotMachineDoAfter(Entity<CoinFliperComponent> ent, ref CoinFlipperDoAfterEvent args)
    {
        if (args.Cancelled) // Almost no way for it to be canceled but just in case
        {
            ent.Comp.IsSpinning = false;
            Dirty(ent);
            return;
        }

        if (args.Handled || !_itemSlots.TryGetSlot(ent.Owner, "money", out var slot))
            return;

        ent.Comp.IsSpinning = false;
        Dirty(ent);

        StackComponent? stack = null;
        if (slot.Item != null)
            TryComp(slot.Item.Value, out stack);

        if (_random.Prob(0.5f))
        {
            _audio.PlayPredicted(ent.Comp.WinSound, ent, args.User);
            if (stack == null)
            {
                var winAmount = ent.Comp.PrizeAmount * 2;
                var newStack = PredictedSpawnAtPosition("SpaceCash", ent.Owner.ToCoordinates());
                if (TryComp<StackComponent>(newStack, out var newStackComp))
                {
                    _stackSystem.SetCount((newStack, newStackComp), winAmount);
                    Dirty(newStack, newStackComp);
                }

                _chatSystem.TrySendInGameICMessage(ent.Owner, Loc.GetString("coinflipper-win", ("amount", winAmount)), InGameICChatType.Speak, hideChat: false, hideLog: true, checkRadioPrefix: false);
                return;
            }
        }

        _audio.PlayPredicted(ent.Comp.LoseSound, ent.Owner, args.User); // If nothing then lose
    }
}
