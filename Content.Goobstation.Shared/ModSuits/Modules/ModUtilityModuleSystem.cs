using Content.Goobstation.Common.Emp;
using Content.Goobstation.Common.Radio;
using Content.Goobstation.Shared.ModSuits.Components;
using Content.Goobstation.Shared.ModSuits.Systems;
using Content.Shared.Clothing.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Emp;
using Content.Shared.Gravity;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModUtilityModuleSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private ItemSlotsSystem _itemSlots = default!;
    [Dependency] private ModSuitSystem _modSuit = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedBatterySystem _battery = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedGravitySystem _gravity = default!;
    [Dependency] private SharedItemSystem _item = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private ThrowingSystem _throwing = default!;

    private const string HatSlotPart = "head";

    [SubscribeLocalEvent]
    private void OnWardEmpAttempt(Entity<ModEmpWardComponent> ent, ref EmpAttemptEvent args)
    {
        args.Cancelled = true;
    }

    [SubscribeLocalEvent]
    private void OnWardContainedEmpAttempt(Entity<ModEmpWardComponent> ent, ref ContainedEmpAttemptEvent args)
    {
        args.Cancelled = true;
    }

    [SubscribeLocalEvent]
    private void OnSignRadio(Entity<ModSignRadioComponent> ent, ref LanguageRadioAttemptEvent args)
    {
        if (ent.Comp.Languages.Count != 0 && !ent.Comp.Languages.Contains(args.Language))
            return;

        args.Allowed = true;
        args.TranslateTo = ent.Comp.TranslateTo;
    }

    [SubscribeLocalEvent]
    private void OnJumpJetUsed(Entity<ModJumpJetModuleComponent> ent, ref ModModuleUsedEvent args)
    {
        var user = args.User;
        var direction = _transform.GetWorldRotation(user).ToWorldVec() * ent.Comp.Distance;
        _throwing.TryThrow(user,
            direction,
            ent.Comp.Speed,
            user,
            pushbackRatio: 0f,
            recoil: false,
            animated: false,
            playSound: false,
            doSpin: false);
        _audio.PlayPredicted(ent.Comp.Sound, user, user);
        args.Handled = true;
    }

    [SubscribeLocalEvent]
    private void OnJointTorsionActivated(Entity<ModJointTorsionModuleComponent> ent, ref ModModuleActivatedEvent args)
    {
        ent.Comp.LastPosition = null;
    }

    private void UpdateJointTorsion()
    {
        var query = EntityQueryEnumerator<ModJointTorsionModuleComponent, ModModuleComponent>();
        while (query.MoveNext(out var uid, out var torsion, out var module))
        {
            if (!module.Active
                || !_modSuit.TryGetModuleSuit(uid, out var suit)
                || suit.Comp.Wearer is not { } wearer
                || _container.IsEntityInContainer(wearer)
                || _gravity.IsWeightless(wearer))
            {
                torsion.LastPosition = null;
                continue;
            }

            var position = _transform.GetMapCoordinates(wearer);
            var last = torsion.LastPosition;
            torsion.LastPosition = position;
            if (last is not { } previous || previous.MapId != position.MapId)
                continue;

            var moved = MathF.Min((position.Position - previous.Position).Length(), torsion.MaxTilesPerUpdate);
            if (moved <= 0.01f || _modSuit.GetBattery(suit) is not { } battery)
                continue;

            _battery.ChangeCharge(battery.AsNullable(), moved * torsion.ChargePerTile);
        }
    }

    [SubscribeLocalEvent]
    private void OnHatStabilizerInstalled(Entity<ModHatStabilizerModuleComponent> ent, ref ModModuleInstalledEvent args)
    {
        if (_timing.ApplyingState
            || !TryComp<ModSuitComponent>(args.Suit, out var suit)
            || _modSuit.GetPart((args.Suit, suit), HatSlotPart) is not { } helmet
            || HasComp<ModHatHolderComponent>(helmet))
            return;

        var holder = EnsureComp<ModHatHolderComponent>(helmet);
        holder.SlotId = ent.Comp.SlotId;
        Dirty(helmet, holder);

        _itemSlots.AddItemSlot(helmet, ent.Comp.SlotId, new ItemSlot
        {
            Whitelist = ent.Comp.Whitelist,
            Name = "modsuit-hat-slot",
            Swap = false,
        });

        ent.Comp.Helmet = helmet;
    }

    [SubscribeLocalEvent]
    private void OnHatStabilizerRemoved(Entity<ModHatStabilizerModuleComponent> ent, ref ModModuleRemovedEvent args)
    {
        if (_timing.ApplyingState || ent.Comp.Helmet is not { } helmet || TerminatingOrDeleted(helmet))
            return;

        ent.Comp.Helmet = null;
        if (_itemSlots.TryGetSlot(helmet, ent.Comp.SlotId, out var slot))
        {
            _itemSlots.TryEject(helmet, slot, null, out _, excludeUserAudio: true);
            _itemSlots.RemoveItemSlot(helmet, slot);
        }

        RemComp<ModHatHolderComponent>(helmet);
        _item.VisualsChanged(helmet);
    }

    [SubscribeLocalEvent]
    private void OnHatInsertAttempt(Entity<ModHatHolderComponent> ent, ref ItemSlotInsertAttemptEvent args)
    {
        if (args.Slot.ID != ent.Comp.SlotId)
            return;

        if (!TryComp<ClothingComponent>(args.Item, out var clothing)
            || (clothing.Slots & SlotFlags.HEAD) == 0
            || HasComp<ModPartComponent>(args.Item))
            args.Cancelled = true;
    }

    [SubscribeLocalEvent]
    private void OnHatInserted(Entity<ModHatHolderComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID == ent.Comp.SlotId)
            _item.VisualsChanged(ent);
    }

    [SubscribeLocalEvent]
    private void OnHatRemoved(Entity<ModHatHolderComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID == ent.Comp.SlotId)
            _item.VisualsChanged(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_net.IsServer)
            UpdateJointTorsion();
    }
}
