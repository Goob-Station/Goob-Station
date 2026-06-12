// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Shared.Alert;
using Content.Shared._Funkystation.MalfAI;
using Content.Shared.Store.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Server-side system that maintains the CPU alert on Malf AI entities.
/// Updates the alert severity to reflect current CPU count.
/// </summary>
public sealed class MalfAiHudServerSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;

    private static readonly ProtoId<AlertPrototype> CpuAlert = "MalfCpu";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfAiMarkerComponent, ComponentStartup>(OnMarkerStartup);
        SubscribeLocalEvent<StoreComponent, StoreCurrencyChangedEvent>(OnStoreCurrencyChanged);
    }

    private void OnMarkerStartup(Entity<MalfAiMarkerComponent> ent, ref ComponentStartup args)
    {
        UpdateCpuAlert(ent.Owner);
    }

    private void OnStoreCurrencyChanged(Entity<StoreComponent> ent, ref StoreCurrencyChangedEvent args)
    {
        if (!HasComp<MalfAiMarkerComponent>(ent.Owner))
            return;

        UpdateCpuAlert(ent.Owner);
    }

    private void UpdateCpuAlert(EntityUid uid)
    {
        if (!TryComp<StoreComponent>(uid, out var store))
        {
            _alerts.ShowAlert(uid, CpuAlert, 0);
            return;
        }

        var cpu = 0;
        if (store.Balance.TryGetValue("CPU", out var cpuVal))
            cpu = (int)cpuVal;

        // Clamp to 0-999 for the 3-digit HUD display
        var severity = (short)Math.Clamp(cpu, 0, 999);
        _alerts.ShowAlert(uid, CpuAlert, severity);
    }
}

/// <summary>
/// Raised on a store entity when its currency balance changes.
/// </summary>
[ByRefEvent]
public record struct StoreCurrencyChangedEvent;
