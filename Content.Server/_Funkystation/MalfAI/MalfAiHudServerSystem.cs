// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Alert;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Prototypes;
using Content.Shared._Gabystation.MalfAi.Components;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Ensures the Malf AI CPU alert is shown at the correct time:
/// - When a CPU-enabled StoreComponent is added to a Malf AI entity.
/// - When the MalfunctioningAiComponent starts on an entity that already has a CPU-enabled store.
/// This prevents ordering issues where the alert would be shown before the store exists.
/// </summary>
public sealed class MalfAiHudServerSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;

    private static readonly ProtoId<CurrencyPrototype> CpuCurrency = "CPU";
    private static readonly ProtoId<AlertPrototype> MalfCpuAlert = "MalfCpu";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StoreComponent, StoreAddedEvent>(OnStoreAdded);
        SubscribeLocalEvent<MalfunctioningAiComponent, ComponentStartup>(OnMalfMarkerStartup);
    }

    private void OnStoreAdded(EntityUid uid, StoreComponent component, ref StoreAddedEvent args)
    {
        // Only show if this store uses CPU and belongs to a Malf AI.
        if (!component.CurrencyWhitelist.Contains(CpuCurrency))
            return;

        if (!HasComp<MalfunctioningAiComponent>(uid))
            return;

        EnsureComp<AlertsComponent>(uid);
        _alerts.ShowAlert(uid, MalfCpuAlert);
    }

    private void OnMalfMarkerStartup(EntityUid uid, MalfunctioningAiComponent component, ComponentStartup args)
    {
        if (!TryComp<StoreComponent>(uid, out var store))
            return;

        if (!store.CurrencyWhitelist.Contains(CpuCurrency))
            return;

        EnsureComp<AlertsComponent>(uid);
        _alerts.ShowAlert(uid, MalfCpuAlert);
    }
}
