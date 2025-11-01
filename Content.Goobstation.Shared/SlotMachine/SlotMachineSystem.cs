using Content.Shared.DoAfter;
using Content.Shared.Verbs;
using Content.Shared.Popups;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Stacks;
using Content.Shared.Chat;
using Content.Shared.Power;
using Content.Shared.Power.EntitySystems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Network;
using Robust.Shared.Timing;
using Robust.Shared.GameObjects;


namespace Content.Goobstation.Shared.SlotMachine
{
    public sealed class SlotMachineSystem : EntitySystem
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
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<SlotMachineComponent, ActivateInWorldEvent>(OnInteractHandEvent);
            SubscribeLocalEvent<SlotMachineComponent, SlotMachineDoAfterEvent>(OnSlotMachineDoAfter);
        }

        /// <summary>
        /// Handle the logic for starting the slot machine
        /// </summary>
        private void OnInteractHandEvent(EntityUid uid, SlotMachineComponent comp, ActivateInWorldEvent args)
        {
            if (comp.IsSpinning || !_power.IsPowered(uid))
                return;

            if (!_itemSlots.TryGetSlot(uid, "money", out var slot)
                || slot.Item == null
                || !TryComp<StackComponent>(slot.Item.Value, out var stack)
                || stack.Count < comp.SpinCost)
            {
                _popupSystem.PopupPredicted(Loc.GetString("slotmachine-no-money"), uid, uid, PopupType.Small); // No Money
                return;
            }

            var doAfter =
             new DoAfterArgs(EntityManager, uid, comp.DoAfterTime, new SlotMachineDoAfterEvent(), uid)
             {
                 BreakOnMove = false,
                 BreakOnDamage = false,
                 MultiplyDelay = false,
             };

            _stackSystem.SetCount(stack.Owner, stack.Count - comp.SpinCost, stack);
            Dirty(stack.Owner, stack);
            comp.IsSpinning = true;

            if (TryComp<AppearanceComponent>(uid, out var appearance))
            {
                _appearance.SetData(uid, SlotMachineVisuals.Spinning, true);
            }
            _doAfter.TryStartDoAfter(doAfter);
            if (_net.IsServer)
                _audio.PlayPvs(comp.SpinSound, uid);
        }

        private void OnSlotMachineDoAfter(EntityUid uid, SlotMachineComponent comp, SlotMachineDoAfterEvent args)
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

            if (TryComp<AppearanceComponent>(uid, out var appearance))
            {
                _appearance.SetData(uid, SlotMachineVisuals.Spinning, false);
            }

            // Handle the chances
            if (slot.Item != null && TryComp<StackComponent>(slot.Item.Value, out var stack)) // Just like real slot machines it will always end on a lose if its your last spin 
            {
                if (_random.Prob(comp.SmallWinChance))
                {
                    _audio.PlayPredicted(comp.SmallWinSound, uid, args.User);
                    HandlePrize(uid, Loc.GetString("slotmachine-win-normal", ("amount", comp.SmallPrizeAmount)), stack, comp.SmallPrizeAmount);
                    return;
                }
                if (_random.Prob(comp.MediumWinChance))
                {
                    _audio.PlayPredicted(comp.MediumWinSound, uid, args.User);
                    HandlePrize(uid, Loc.GetString("slotmachine-win-normal", ("amount", comp.MediumPrizeAmount)), stack, comp.MediumPrizeAmount);
                    return;
                }
                if (_random.Prob(comp.BigWinChance))
                {
                    _audio.PlayPredicted(comp.BigWinSound, uid, args.User);
                    HandlePrize(uid, Loc.GetString("slotmachine-win-normal", ("amount", comp.BigPrizeAmount)), stack, comp.BigPrizeAmount);
                    return;
                }
                if (_random.Prob(comp.JackPotWinChance))
                {
                    _audio.PlayPredicted(comp.JackPotWinSound, uid, args.User);
                    HandlePrize(uid, Loc.GetString("slotmachine-win-jackpot"), stack, comp.JackPotPrizeAmount);
                    return;
                }
                if (_random.Prob(comp.GodPotWinChance)) // THE GODPOT!!!
                {
                    _audio.PlayPredicted(comp.GodPotWinSound, uid, args.User);
                    var coordinates = Transform(uid).Coordinates;
                    EntityManager.SpawnEntity(comp.GodPotPrize, coordinates);
                    _chatSystem.TrySendInGameICMessage(uid, Loc.GetString("slotmachine-win-godpot"), InGameICChatType.Speak, hideChat: false, hideLog: true, checkRadioPrefix: false);
                    return;
                }
            }

            _audio.PlayPredicted(comp.LoseSound, uid, args.User); // If nothing then lose
        }
        private void HandlePrize(EntityUid uid, string msg, StackComponent stack, int prize)
        {
            // Add money to the stack and play a message
            _stackSystem.SetCount(stack.Owner, stack.Count + prize, stack);
            Dirty(stack.Owner, stack);
            _chatSystem.TrySendInGameICMessage(uid, msg, InGameICChatType.Speak, hideChat: false, hideLog: true, checkRadioPrefix: false);
        }
    }
}