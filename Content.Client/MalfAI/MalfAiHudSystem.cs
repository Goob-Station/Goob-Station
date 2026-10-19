// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.Alerts;
using Content.Shared.Alert;
using Content.Shared.MalfAI;
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
        // Subscribe on StationAiHeld so this runs for the local AI eye entity.
        SubscribeLocalEvent<Content.Shared.Silicons.StationAi.StationAiHeldComponent, UpdateAlertSpriteEvent>(OnUpdateAlert);
    }

    private EntityUid? ResolveMalfAiEntity(EntityUid local)
    {
        // Prefer local if it already has a store (covers some setups)
        if (TryComp<StoreComponent>(local, out _))
            return local;

        // Find any entity flagged as Malf AI that also has a store.
        var query = AllEntityQuery<MalfAiMarkerComponent, StoreComponent>();
        while (query.MoveNext(out var uid, out _, out _))
            return uid;

        return null;
    }

    private void OnUpdateAlert(Entity<Content.Shared.Silicons.StationAi.StationAiHeldComponent> ent, ref UpdateAlertSpriteEvent args)
    {
        if (args.Alert.ID != "MalfCpu")
            return;

        // Find which entity holds the CPU store.
        var source = ResolveMalfAiEntity(ent.Owner);
        if (source == null || !TryComp<StoreComponent>(source.Value, out var store))
            return;

        var sprite = args.SpriteViewEnt.Comp;

        // Read CPU amount and clamp to 0..999
        ProtoId<CurrencyPrototype> cpu = "CPU";
        var amount = 0;
        if (store.Balance.TryGetValue(cpu, out FixedPoint2 val))
            amount = (int) val.Int();
        amount = Math.Clamp(amount, 0, 999);

        sprite.LayerSetState(MalfAiHudVisualLayers.Digit1, $"{(amount / 100) % 10}");
        sprite.LayerSetState(MalfAiHudVisualLayers.Digit2, $"{(amount / 10) % 10}");
        sprite.LayerSetState(MalfAiHudVisualLayers.Digit3, $"{amount % 10}");
    }
}
