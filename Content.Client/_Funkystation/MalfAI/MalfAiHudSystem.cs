// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Client.Alerts;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Funkystation.MalfAI;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Client._Funkystation.MalfAI;

/// <summary>
/// Renders the Malf AI CPU amount as a 3-digit readout on the MalfCpu alert icon.
/// </summary>
public sealed class MalfAiHudSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    private static readonly ProtoId<CurrencyPrototype> CpuCurrency = "CPU";

    public override void Initialize()
    {
        base.Initialize();
        // Subscribe on StationAiHeld so this runs for the local AI entity.
        SubscribeLocalEvent<StationAiHeldComponent, UpdateAlertSpriteEvent>(OnUpdateAlert);
    }

    private EntityUid? ResolveMalfAiEntity(EntityUid local)
    {
        // Prefer local if it already has a store (covers some setups)
        if (HasComp<StoreComponent>(local))
            return local;

        // Find any entity flagged as Malf AI that also has a store.
        var query = AllEntityQuery<MalfAiMarkerComponent, StoreComponent>();
        while (query.MoveNext(out var uid, out _, out _))
            return uid;

        return null;
    }

    private void OnUpdateAlert(Entity<StationAiHeldComponent> ent, ref UpdateAlertSpriteEvent args)
    {
        if (args.Alert.ID != "MalfCpu")
            return;

        // Find which entity holds the CPU store.
        var source = ResolveMalfAiEntity(ent.Owner);
        if (source == null || !TryComp<StoreComponent>(source.Value, out var store))
            return;

        // Read CPU amount and clamp to 0..999
        var amount = 0;
        if (store.Balance.TryGetValue(CpuCurrency, out FixedPoint2 val))
            amount = (int) val.Int();
        amount = Math.Clamp(amount, 0, 999);

        var spriteView = args.SpriteViewEnt;
        SetDigit(spriteView, MalfAiHudVisualLayers.Digit1, (amount / 100) % 10);
        SetDigit(spriteView, MalfAiHudVisualLayers.Digit2, (amount / 10) % 10);
        SetDigit(spriteView, MalfAiHudVisualLayers.Digit3, amount % 10);
    }

    private void SetDigit(Entity<SpriteComponent> spriteView, MalfAiHudVisualLayers key, int digit)
    {
        if (!_sprite.LayerMapTryGet((spriteView.Owner, spriteView.Comp), key, out var layer, false))
            return;

        _sprite.LayerSetRsiState((spriteView.Owner, spriteView.Comp), layer, digit.ToString());
    }
}
