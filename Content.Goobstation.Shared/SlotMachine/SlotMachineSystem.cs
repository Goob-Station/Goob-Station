using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.Interaction;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Stacks;
using Content.Shared.Emag.Systems;
using Content.Shared.Power.EntitySystems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Content.Shared.Random.Helpers;
using Robust.Shared.Timing;
using Content.Shared.Administration.Logs;

namespace Content.Goobstation.Shared.SlotMachine;

public sealed partial class SlotMachineSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
    [Dependency] private readonly SharedStackSystem _stackSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PrizeSystem _prize = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLog = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlotMachineComponent, ActivateInWorldEvent>(OnInteractHandEvent);
        SubscribeLocalEvent<SlotMachineComponent, SlotMachineDoAfterEvent>(OnSlotMachineDoAfter);
        SubscribeLocalEvent<SlotMachineComponent, SlotMachineEmagDoAfterEvent>(OnSlotMachineEmagDoAfter);
        SubscribeLocalEvent<SlotMachineComponent, GotEmaggedEvent>(OnEmagged);
    }

    private void SetSpinning(Entity<SlotMachineComponent> ent, bool spinning)
    {
        _appearance.SetData(ent, SlotMachineVisuals.Spinning, false);
        ent.Comp.IsSpinning = spinning;
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

        _audio.PlayPredicted(ent.Comp.SpinSound, ent, ent);
        _doAfter.TryStartDoAfter(doAfter);

        SetSpinning(ent, true);
        Dirty(ent);
    }

    private void OnSlotMachineDoAfter(Entity<SlotMachineComponent> ent, ref SlotMachineDoAfterEvent args)
    {
        if (args.Handled)
            return;

        if (!args.Cancelled) // Almost no way for it to be canceled but just in case
        {
            var random = SharedRandomExtensions.PredictedRandom(_timing, GetNetEntity(ent));
            _prize.HandlePrize(ent.Comp.Prizes, ent.Owner, random);
        }

        SetSpinning(ent, false);
        Dirty(ent);
    }
}
