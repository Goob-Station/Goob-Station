using System.Linq;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.Interaction;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Stacks;
using Content.Shared.Chat;
using Content.Shared.Coordinates;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.EntityTable;
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
        [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
        [Dependency] private readonly SharedStackSystem _stackSystem = default!;
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
        [Dependency] private readonly IPrototypeManager _proto = default!;
        [Dependency] private readonly PrizeSystem _prize = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<SlotMachineComponent, ActivateInWorldEvent>(OnInteractHandEvent);
            SubscribeLocalEvent<SlotMachineComponent, SlotMachineDoAfterEvent>(OnSlotMachineDoAfter);
            SubscribeLocalEvent<SlotMachineComponent, SlotMachineEmagDoAfterEvent>(OnSlotMachineEmagDoAfter);
            SubscribeLocalEvent<SlotMachineComponent, GotEmaggedEvent>(OnEmagged);
        }

        /// <summary>
        /// Spawns a random entity when emmaged
        /// </summary>
        private void OnEmagged(Entity<SlotMachineComponent> ent, ref GotEmaggedEvent args)
        {
            if (HasComp<EmaggedComponent>(ent.Owner))
                return;

            args.Handled = true;
            EnsureComp<EmaggedComponent>(ent);

            var entities = _proto.EnumeratePrototypes<EntityPrototype>().ToList();
            ent.Comp.EmagSpawnEntity = _random.Pick(entities).ID;

            var doAfter =
                new DoAfterArgs(EntityManager, ent.Owner, ent.Comp.DoAfterTime, new SlotMachineEmagDoAfterEvent(), ent.Owner)
                {
                    BreakOnMove = false,
                    BreakOnDamage = false,
                    MultiplyDelay = false,
                };

            ent.Comp.IsSpinning = true;

            if (_net.IsServer)
            {
                _audio.PlayPvs(ent.Comp.SpinSound, ent.Owner);
                _doAfter.TryStartDoAfter(doAfter);
                _appearance.SetData(ent.Owner, SlotMachineVisuals.Spinning, true);
            }
        }

        private void OnSlotMachineEmagDoAfter(Entity<SlotMachineComponent> ent, ref SlotMachineEmagDoAfterEvent args)
        {
            if (ent.Comp.EmagSpawnEntity is not null)
            {
                _appearance.SetData(ent.Owner, SlotMachineVisuals.Spinning, false);
                PredictedSpawnAtPosition(ent.Comp.EmagSpawnEntity, ent.Owner.ToCoordinates());
            }

            ent.Comp.IsSpinning = false;
            Dirty(ent);
        }

        /// <summary>
        /// Handle the logic for starting the slot machine
        /// </summary>
        private void OnInteractHandEvent(Entity<SlotMachineComponent> ent, ref ActivateInWorldEvent args)
        {
            if (ent.Comp.IsSpinning || !_power.IsPowered(ent.Owner))
                return;

            if (!_itemSlots.TryGetSlot(ent.Owner, "money", out var slot)
                || slot.Item is not { } item
                || _stackSystem.GetCount(item) < ent.Comp.SpinCost)
            {
                _popupSystem.PopupPredicted(Loc.GetString("slotmachine-no-money"), ent.Owner, args.User); // No Money
                return;
            }

            var doAfter =
             new DoAfterArgs(EntityManager, ent.Owner, ent.Comp.DoAfterTime, new SlotMachineDoAfterEvent(), ent.Owner)
             {
                 BreakOnMove = false,
                 BreakOnDamage = false,
                 MultiplyDelay = false,
             };

            if (TryComp<StackComponent>(item, out var stack))
                _stackSystem.SetCount((item, stack), _stackSystem.GetCount(item) - ent.Comp.SpinCost);

            ent.Comp.IsSpinning = true;

            if (_net.IsServer) // The DoAfter causes a weird jitter if its predicted for some reason
            {
                _audio.PlayPvs(ent.Comp.SpinSound, ent.Owner);
                _doAfter.TryStartDoAfter(doAfter);
                _appearance.SetData(ent.Owner, SlotMachineVisuals.Spinning, true);
            }
        }

        private void OnSlotMachineDoAfter(Entity<SlotMachineComponent> ent, ref SlotMachineDoAfterEvent args)
        {
            if (args.Handled)
                return;

            if (args.Cancelled) // Almost no way for it to be canceled but just in case
            {
                ent.Comp.IsSpinning = false;
                Dirty(ent);
                return;
            }

            ent.Comp.IsSpinning = false;
            Dirty(ent);

            _appearance.SetData(ent.Owner, SlotMachineVisuals.Spinning, false);

            _prize.HandlePrize(ent.Comp.Prizes, ent.Owner);
        }
    }
}
