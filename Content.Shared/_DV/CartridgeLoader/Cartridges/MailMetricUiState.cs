// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class MailMetricUiState : BoundUserInterfaceState
{
    public readonly MailStats Metrics;
    public int UnopenedMailCount { get; }
    public int TotalMail { get; }
    public double SuccessRate { get; }

    public MailMetricUiState(MailStats metrics, int unopenedMailCount)
    {
        Metrics = metrics;
        UnopenedMailCount = unopenedMailCount;
        TotalMail = metrics.TotalMail(unopenedMailCount);
        SuccessRate = metrics.SuccessRate(unopenedMailCount);
    }
}

[DataDefinition]
[Serializable, NetSerializable]
public partial record struct MailStats
{
    public int Earnings { get; init; }
    public int DamagedLosses { get; init; }
    public int ExpiredLosses { get; init; }
    public int TamperedLosses { get; init; }
    public int OpenedCount { get; init; }
    public int DamagedCount { get; init; }
    public int ExpiredCount { get; init; }
    public int TamperedCount { get; init; }

    public readonly int TotalMail(int unopenedCount)
    {
        return OpenedCount + unopenedCount;
    }

    public readonly int TotalIncome => Earnings + DamagedLosses + ExpiredLosses + TamperedLosses;

    public readonly double SuccessRate(int unopenedCount)
    {
        var totalMail = TotalMail(unopenedCount);
        return (totalMail > 0)
            ? Math.Round((double)OpenedCount / totalMail * 100, 2)
            : 0;
    }
}