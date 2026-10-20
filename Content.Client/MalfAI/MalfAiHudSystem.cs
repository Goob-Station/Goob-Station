// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.Alerts;
using Content.Shared.Alert;
using Content.Shared.Alert.Components;
using Content.Shared.MalfAI;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Store.Components;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Store;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Client.MalfAI;

public sealed class MalfAiHudSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AlertsComponent, GetGenericAlertCounterAmountEvent>(OnGetCounterAmount);
        SubscribeLocalEvent<StoreComponent, GetGenericAlertCounterAmountEvent>(OnGetCounterAmountStore);
        SubscribeLocalEvent<StationAiHeldComponent, GetGenericAlertCounterAmountEvent>(OnGetCounterAmountHeld);
    }

    private EntityUid? ResolveMalfAiEntity(EntityUid local)
    {
        // 1. Prefer local entity if it has a store with CPU
        if (TryComp<StoreComponent>(local, out var localStore) &&
            (localStore.Balance.ContainsKey("CPU") || localStore.CurrencyWhitelist.Contains("CPU")))
        {
            return local;
        }

        // 2. If local is an AI held entity
        if (TryComp<StationAiHeldComponent>(local, out _) && TryComp<StoreComponent>(local, out _))
            return local;

        // 3. Find any entity flagged as Malf AI that also has a store.
        var query = AllEntityQuery<MalfAiMarkerComponent, StoreComponent>();
        while (query.MoveNext(out var uid, out _, out _))
            return uid;

        // 4. Fall back to any store that contains CPU in balance or whitelist
        var storeQuery = AllEntityQuery<StoreComponent>();
        while (storeQuery.MoveNext(out var uid, out var store))
        {
            if (store.Balance.ContainsKey("CPU") || store.CurrencyWhitelist.Contains("CPU"))
                return uid;
        }

        return null;
    }

    private void OnGetCounterAmountStore(Entity<StoreComponent> ent, ref GetGenericAlertCounterAmountEvent args)
    {
        TryHandleCounter(ent.Owner, ref args);
    }

    private void OnGetCounterAmountHeld(Entity<StationAiHeldComponent> ent, ref GetGenericAlertCounterAmountEvent args)
    {
        TryHandleCounter(ent.Owner, ref args);
    }

    private void OnGetCounterAmount(Entity<AlertsComponent> ent, ref GetGenericAlertCounterAmountEvent args)
    {
        TryHandleCounter(ent.Owner, ref args);
    }

    private void TryHandleCounter(EntityUid local, ref GetGenericAlertCounterAmountEvent args)
    {
        if (args.Handled)
            return;

        if (args.Alert.ID != "MalfCpu")
            return;

        var source = ResolveMalfAiEntity(local);
        if (source == null || !TryComp<StoreComponent>(source.Value, out var store))
        {
            args.Amount = 0;
            return;
        }

        ProtoId<CurrencyPrototype> cpu = "CPU";
        if (store.Balance.TryGetValue(cpu, out FixedPoint2 val))
            args.Amount = (int) val.Int();
        else
            args.Amount = 0;
    }
}
