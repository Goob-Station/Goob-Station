// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Cargo;
using Content.Shared.CartridgeLoader.Cartridges;

namespace Content.Server._DV.Cargo.Components;

/// <summary>
/// Added to the abstract representation of a station to track stats related to mail delivery and income
/// </summary>
[RegisterComponent, Access(typeof(SharedCargoSystem))]
public sealed partial class StationLogisticStatsComponent : Component
{
    [DataField]
    public MailStats Metrics { get; set; }
}