// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.ItemMiner;
using Content.Goobstation.Shared.ItemMiner;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Stacks;
using Content.Server.Power.EntitySystems;
using Content.Server.Stack;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Timing;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.ItemMiner;

public sealed class ItemMinerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly PowerReceiverSystem _power = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly StackSystem _stack = default!;

    // no freezing the game
    private TimeSpan _minInterval = TimeSpan.FromSeconds(0.001f);

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ItemMinerComponent, MapInitEvent>(OnInit);
    }

    private void OnInit(Entity<ItemMinerComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextAt = _timing.CurTime + ent.Comp.Interval;
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<ItemMinerComponent, TransformComponent>();

        // see if this ends up having a performance impact, maybe update every 0.5s and do batch-spawning if so
        while (query.MoveNext(out var uid, out var miner, out var xform))
        {
            var checkEv = new ItemMinerCheckEvent();
            RaiseLocalEvent(uid, ref checkEv);

            if (checkEv.Cancelled
                || miner.NeedsPower && !_power.IsPowered(uid)
                || miner.NeedsAnchored && !xform.Anchored)
            {
                miner.NextAt += TimeSpan.FromSeconds(frameTime);
                QueueDel(miner.AudioUid);
                continue;
            }

            if (TerminatingOrDeleted(miner.AudioUid) && miner.MiningSound != null)
                miner.AudioUid = _audio.PlayPvs(miner.MiningSound, uid)?.Entity;

            if (miner.NextAt < _timing.CurTime)
            {
                if (miner.Interval < _minInterval)
                {
                    Log.Error($"Item miner {ToPrettyString(uid)} had very low interval {miner.Interval}, change Amount instead");
                    miner.NextAt += TimeSpan.FromSeconds(1f); // don't spam it
                    continue;
                }

                miner.NextAt += miner.Interval;

                // how much we actually spawned
                var spawned = 0;

                ItemSlot? slot = null;
                if (miner.ItemSlotId != null && !_itemSlots.TryGetSlot(uid, miner.ItemSlotId, out slot))
                {
                    Log.Error($"Item miner {ToPrettyString(uid)} lacks item slot {miner.ItemSlotId}");
                    miner.NextAt += TimeSpan.FromSeconds(1f); // don't spam it
                    continue;
                }

                EntityUid? slotItem = slot == null ? null : slot.Item;
                if (slot != null && slotItem == null)
                {
                    var slotEnt = Spawn(miner.Proto, xform.Coordinates);
                    if (slot.ContainerSlot == null || !_container.Insert(slotEnt, slot.ContainerSlot))
                    {
                        Log.Error($"Item miner {ToPrettyString(uid)} failed to insert newly spawned entity into slot {miner.ItemSlotId}");
                        miner.NextAt += TimeSpan.FromSeconds(1f); // don't spam it
                        QueueDel(slotEnt);
                        continue;
                    }
                    slotItem = slotEnt;
                    spawned++;
                }

                EntityUid minedUid = slotItem ?? Spawn(miner.Proto, xform.Coordinates);
                if (slotItem == null)
                    spawned++;

                if (TryComp<StackComponent>(minedUid, out var stack))
                {
                    // check if we're spawning a stack proto with a non-1 count
                    var remaining = miner.Amount - spawned;
                    var entProto = _proto.Index(miner.Proto);

                    // from here on `spawned` and `amt` stand for stack counts and not entity counts
                    if (entProto.TryGetComponent<StackComponent>(out var stackComp, EntityManager.ComponentFactory))
                    {
                        spawned *= stackComp.Count;
                        remaining *= stackComp.Count;
                    }

                    var count = stack.Count;
                    _stack.SetCount(minedUid, count + remaining, stack);
                    spawned += stack.Count - count;
                }
                else if (slot == null)
                {
                    for (var i = 0; i < miner.Amount - spawned; i++)
                        Spawn(miner.Proto, xform.Coordinates);
                    spawned = miner.Amount;
                }
                // if it's not stackable and we're using a slot, just don't spawn more than 1
                // TODO: bool for spillover? not needed right now

                var ev = new ItemMinedEvent(minedUid, spawned);
                RaiseLocalEvent(uid, ev);
            }
        }
    }
}
