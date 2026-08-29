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
        [Dependency] private readonly SharedChatSystem _chatSystem = default!;
        [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
        [Dependency] private readonly SharedStackSystem _stackSystem = default!;
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
        [Dependency] private readonly IPrototypeManager _proto = default!;
        [Dependency] private readonly EntityTableSystem _entityTable = default!;

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
            if (_net.IsServer && ent.Comp.EmagSpawnEntity is not null)
            {
                _appearance.SetData(ent.Owner, SlotMachineVisuals.Spinning, false);
                Spawn(ent.Comp.EmagSpawnEntity, ent.Owner.ToCoordinates());
            }

            ent.Comp.IsSpinning = false;
            Dirty(ent);
        }

        /// <summary>
        /// Handle the logic for starting the slot machine
        /// </summary>

        private void OnInteractHandEvent(EntityUid uid, SlotMachineComponent comp, ActivateInWorldEvent args)
        {
            if (comp.IsSpinning || !_power.IsPowered(uid))
                return;

            if (!_itemSlots.TryGetSlot(uid, "money", out var slot)
                || slot.Item is not { } item
                || _stackSystem.GetCount(item) < comp.SpinCost)
            {
                _popupSystem.PopupPredicted(Loc.GetString("slotmachine-no-money"), uid, args.User); // No Money
                return;
            }

            var doAfter =
             new DoAfterArgs(EntityManager, uid, comp.DoAfterTime, new SlotMachineDoAfterEvent(), uid)
             {
                 BreakOnMove = false,
                 BreakOnDamage = false,
                 MultiplyDelay = false,
             };

            if (TryComp<StackComponent>(item, out var stack))
                _stackSystem.SetCount((item, stack), _stackSystem.GetCount(item) - comp.SpinCost);

            comp.IsSpinning = true;

            if (_net.IsServer) // DoAfters cause misperdicts?
            {
                _audio.PlayPvs(comp.SpinSound, uid);
                _doAfter.TryStartDoAfter(doAfter);
                _appearance.SetData(uid, SlotMachineVisuals.Spinning, true);
            }
        }

        private void OnSlotMachineDoAfter(Entity<SlotMachineComponent> ent, ref SlotMachineDoAfterEvent args)
        {
            if (args.Cancelled) // Almost no way for it to be canceled but just in case
            {
                ent.Comp.IsSpinning = false;
                Dirty(ent);
                return;
            }

            if (args.Handled)
                return;

            ent.Comp.IsSpinning = false;
            Dirty(ent);

            _appearance.SetData(ent.Owner, SlotMachineVisuals.Spinning, false);

            var prize = GetRandomPrize();

            HandlePrize(ent, prize);
        }
        private void HandlePrize(Entity<SlotMachineComponent> ent, PrizePrototype prize)
        {
            var win = _entityTable.GetSpawns(prize.PrizeTable);

            foreach (var item in win)
            {
                Spawn(item, ent.Owner.ToCoordinates());
            }

            _audio.PlayPredicted(prize.WinSound, ent, ent);
            if (prize.WinMessage is not null)
                _chatSystem.TrySendInGameICMessage(ent, prize.WinMessage, InGameICChatType.Speak, hideChat: false, hideLog: true, checkRadioPrefix: false);
        }

        public PrizePrototype GetRandomPrize()
        {
            var query = _proto.EnumeratePrototypes<PrizePrototype>();

            Dictionary<PrizePrototype, float> picks = new();
            foreach (var fill in query)
            {
                picks[fill] = fill.Weight;
            }

            var sum = picks.Values.Sum();
            var accumulated = 0f;

            var rand = _random.NextFloat() * sum;

            foreach (var (prize, weight) in picks)
            {
                accumulated += weight;

                if (accumulated >= rand)
                {
                    return prize;
                }
            }

            throw new InvalidOperationException("Unable to find weighted random for a slot machine prize (THIS SHOULDN'T BE POSSIBLE)");
        }
    }
}
