using Content.Goobstation.Shared.Doodon.Building;
using Content.Goobstation.Shared.Doodons;
using Content.Server.Popups;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Stacks;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using System;

namespace Content.Goobstation.Server.Doodon.Building;

public sealed class DoodonBuildSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    private const string ResinStackTypeId = "DoodonResin";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DoodonBuildWorldActionEvent>(OnPlaceWorld);
    }

    private void OnPlaceWorld(DoodonBuildWorldActionEvent args)
    {
        if (args.Handled)
            return;

        var performer = args.Performer;

        if (!TryComp<DoodonBuilderComponent>(performer, out var builder))
            return;

        _popup.PopupEntity("OnPlaceWorld fired", performer, performer);

        var selected = builder.GetSelected();
        if (selected is null)
        {
            _popup.PopupEntity("Select a structure first.", performer, performer);
            return;
        }

        var coords = args.Target;

        if (!InRange(performer, coords, builder.MaxBuildRange))
            return;

        if (!TryGetBuildCost(selected.Value, out var cost))
            return;

        if (cost > 0 && !TryConsumeResin(performer, cost))
        {
            var nameFail = _proto.TryIndex<EntityPrototype>(selected.Value, out var pFail)
                ? pFail.Name
                : selected.Value.ToString();

            _popup.PopupEntity($"Not enough resin to build {nameFail} (need {cost}).", performer, performer);
            return;
        }

        Spawn(selected.Value, coords);
        args.Handled = true;

        var nameOk = _proto.TryIndex<EntityPrototype>(selected.Value, out var pOk)
            ? pOk.Name
            : selected.Value.ToString();

        _popup.PopupEntity(cost > 0 ? $"Built {nameOk} (-{cost} resin)." : $"Built {nameOk}.", performer, performer);
    }

    private bool InRange(EntityUid user, EntityCoordinates target, float range)
    {
        if (range <= 0f)
            return true;

        var userPos = _xform.GetWorldPosition(user);
        var targetPos = _xform.ToMapCoordinates(target).Position;
        return (userPos - targetPos).Length() <= range;
    }

    private bool TryGetBuildCost(EntProtoId protoId, out int cost)
    {
        cost = 0;
        if (!_proto.TryIndex<EntityPrototype>(protoId, out var proto))
            return false;

        if (proto.TryGetComponent(out DoodonBuildingComponent? buildingComp))
        {
            cost = Math.Max(0, buildingComp.ResinCost);
            return true;
        }

        return false;
    }

    private bool TryConsumeResin(EntityUid builder, int amount)
    {
        if (amount <= 0)
            return true;

        // Pass 1: count without consuming
        var total = CountResin(builder);
        if (total < amount)
            return false;

        // Pass 2: consume now that we know we can pay
        var remaining = amount;

        foreach (var held in _hands.EnumerateHeld(builder))
        {
            if (remaining <= 0) break;
            TryConsumeFromStackEntity(held, ref remaining);
        }

        if (remaining > 0)
        {
            var slots = _inventory.GetSlotEnumerator(builder);
            while (slots.MoveNext(out var slot))
            {
                if (remaining <= 0) break;

                var item = slot.ContainedEntity;
                if (item is { } itemUid)
                    TryConsumeFromStackEntity(itemUid, ref remaining);
            }
        }

        return true; // guaranteed by CountResin
    }

    private void TryConsumeFromStackEntity(EntityUid uid, ref int remaining)
    {
        if (remaining <= 0)
            return;

        if (!TryComp<StackComponent>(uid, out var stack))
            return;

        if (!string.Equals(stack.StackTypeId, ResinStackTypeId, StringComparison.Ordinal))
            return;

        var take = Math.Min(stack.Count, remaining);
        if (take <= 0)
            return;

        stack.Count -= take;
        Dirty(uid, stack);

        if (stack.Count <= 0)
            QueueDel(uid);

        remaining -= take;
    }

    private int CountResin(EntityUid builder)
    {
        var total = 0;

        foreach (var held in _hands.EnumerateHeld(builder))
            total += CountResinInEntity(held);

        var slots = _inventory.GetSlotEnumerator(builder);
        while (slots.MoveNext(out var slot))
        {
            var item = slot.ContainedEntity;
            if (item is { } itemUid)
                total += CountResinInEntity(itemUid);
        }

        return total;
    }

    private int CountResinInEntity(EntityUid uid)
    {
        if (!TryComp<StackComponent>(uid, out var stack))
            return 0;

        if (!string.Equals(stack.StackTypeId, ResinStackTypeId, StringComparison.Ordinal))
            return 0;

        return Math.Max(0, stack.Count);
    }
}
