// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Shipyard.Prototypes;

/// <summary>
/// Like <c>TagPrototype</c> but for vessel categories.
/// Prevents making typos being silently ignored by the linter.
/// </summary>
[Prototype("vesselCategory")]
public sealed class VesselCategoryPrototype : IPrototype
{
    [ViewVariables, IdDataField]
    public string ID { get; } = default!;
}
