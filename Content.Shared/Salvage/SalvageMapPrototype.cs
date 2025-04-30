// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tayrtahn <tayrtahn@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Salvage;

[Prototype]
public sealed partial class SalvageMapPrototype : IPrototype
{
    [ViewVariables] [IdDataField] public string ID { get; private set; } = default!;

    /// <summary>
    /// Relative directory path to the given map, i.e. `Maps/Salvage/template.yml`
    /// </summary>
    [DataField(required: true)] public ResPath MapPath;

    /// <summary>
    /// String that describes the size of the map.
    /// </summary>
    [DataField(required: true)]
    public LocId SizeString;
}