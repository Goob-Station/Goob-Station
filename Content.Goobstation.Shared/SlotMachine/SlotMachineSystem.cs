using System.Linq;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.Interaction;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Stacks;
using Content.Shared.Chat;
using Content.Shared.Emag.Systems;
using Content.Shared.Power.EntitySystems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;


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
        [Dependency] private readonly IPrototypeManager _proto = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<SlotMachineComponent, ActivateInWorldEvent>(OnInteractHandEvent);
            SubscribeLocalEvent<SlotMachineComponent, SlotMachineDoAfterEvent>(OnSlotMachineDoAfter);
            SubscribeLocalEvent<SlotMachineComponent, GotEmaggedEvent>(OnEmagged);
        }

        /// <summary>
        /// For whenever it gets emagged
        /// </summary>
        private void OnEmagged(EntityUid uid, SlotMachineComponent comp, ref GotEmaggedEvent args)
        {
            if(comp.Emagged)
                return;

            args.Handled = true;
            comp.Emagged = true;

            comp.SpinCost = _random.Next(50, 100000);
            comp.SmallPrizeAmount = _random.Next(-500, 5000);
            comp.MediumPrizeAmount = _random.Next(-500, 10000);
            comp.BigPrizeAmount = _random.Next(-500, 50000);
            comp.JackPotPrizeAmount = _random.Next(-500, 100000);

            comp.SmallWinChance  = _random.NextFloat(0, 0.6f);
            comp.MediumWinChance  = _random.NextFloat(0, 0.35f);
            comp.BigWinChance  = _random.NextFloat(0f, 0.2f);
            comp.JackPotWinChance  = _random.NextFloat(0, 0.1f);
            comp.GodPotWinChance =  _random.NextFloat(0, 0.05f);

            // lord have mercy...
            var allProtos = _proto.EnumeratePrototypes<EntityPrototype>().ToList();

            if (allProtos.Count > 0)
            {
                var randomProto = _random.Pick(allProtos);
                comp.GodPotPrize = randomProto.ID;
            }
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

            if (_net.IsServer)
            {
                _audio.PlayPvs(comp.SpinSound, uid);
                _doAfter.TryStartDoAfter(doAfter);
            }
            if (TryComp<AppearanceComponent>(uid, out var appearance) && _net.IsServer)
            {
                _appearance.SetData(uid, SlotMachineVisuals.Spinning, true);
            }
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

            if (TryComp<AppearanceComponent>(uid, out var appearance) && _net.IsServer)
            {
                _appearance.SetData(uid, SlotMachineVisuals.Spinning, false);
            }

            // Handle the chances
            StackComponent? stack = null;
            if (slot.Item != null)
                TryComp<StackComponent>(slot.Item.Value, out stack);

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

            _audio.PlayPredicted(comp.LoseSound, uid, args.User); // If nothing then lose
        }
        private void HandlePrize(EntityUid uid, string msg, StackComponent? stack, int prize)
        {
            if (stack == null)
            {
                // Spawn a new cash stack if there's no money left in the machine
                var coordinates = Transform(uid).Coordinates;
                var newStack = EntityManager.SpawnEntity("SpaceCash", coordinates);
                if (TryComp<StackComponent>(newStack, out var newStackComp))
                {
                    _stackSystem.SetCount(newStack, prize, newStackComp);
                    Dirty(newStack, newStackComp);
                }

                _chatSystem.TrySendInGameICMessage(uid, msg, InGameICChatType.Speak, hideChat: false, hideLog: true, checkRadioPrefix: false);
                return;
            }

            // Add money to the stack and play a message
            _stackSystem.SetCount(stack.Owner, stack.Count + prize, stack);
            Dirty(stack.Owner, stack);
            _chatSystem.TrySendInGameICMessage(uid, msg, InGameICChatType.Speak, hideChat: false, hideLog: true, checkRadioPrefix: false);
        }
    }
}
