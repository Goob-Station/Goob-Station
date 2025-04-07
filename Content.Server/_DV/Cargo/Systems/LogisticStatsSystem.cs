// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._DV.Cargo.Components;
using Content.Shared.Cargo;
using JetBrains.Annotations;

namespace Content.Server._DV.Cargo.Systems;

public sealed partial class LogisticStatsSystem : SharedCargoSystem
{
    public override void Initialize()
    {
        base.Initialize();
    }

    [PublicAPI]
    public void AddOpenedMailEarnings(EntityUid uid, StationLogisticStatsComponent component, int earnedMoney)
    {
        component.Metrics = component.Metrics with
        {
            Earnings = component.Metrics.Earnings + earnedMoney,
            OpenedCount = component.Metrics.OpenedCount + 1
        };
        UpdateLogisticsStats(uid);
    }

    [PublicAPI]
    public void AddExpiredMailLosses(EntityUid uid, StationLogisticStatsComponent component, int lostMoney)
    {
        component.Metrics = component.Metrics with
        {
            ExpiredLosses = component.Metrics.ExpiredLosses + lostMoney,
            ExpiredCount = component.Metrics.ExpiredCount + 1
        };
        UpdateLogisticsStats(uid);
    }

    [PublicAPI]
    public void AddDamagedMailLosses(EntityUid uid, StationLogisticStatsComponent component, int lostMoney)
    {
        component.Metrics = component.Metrics with
        {
            DamagedLosses = component.Metrics.DamagedLosses + lostMoney,
            DamagedCount = component.Metrics.DamagedCount + 1
        };
        UpdateLogisticsStats(uid);
    }

    [PublicAPI]
    public void AddTamperedMailLosses(EntityUid uid, StationLogisticStatsComponent component, int lostMoney)
    {
        component.Metrics = component.Metrics with
        {
            TamperedLosses = component.Metrics.TamperedLosses + lostMoney,
            TamperedCount = component.Metrics.TamperedCount + 1
        };
        UpdateLogisticsStats(uid);
    }

    private void UpdateLogisticsStats(EntityUid uid) => RaiseLocalEvent(new LogisticStatsUpdatedEvent(uid));
}

public sealed class LogisticStatsUpdatedEvent : EntityEventArgs
{
    public EntityUid Station;
    public LogisticStatsUpdatedEvent(EntityUid station)
    {
        Station = station;
    }
}