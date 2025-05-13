// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.StationEvents;

[RegisterComponent, Access(typeof(AddComponentRandomRule))]
public sealed partial class AddComponentRandomRuleComponent : Component
{
    /// <summary>
    /// Components to add
    /// </summary>
    [DataField(required: true)]
    public ComponentRegistry Components = new();

    /// <summary>
    /// If an entity matches this blacklist, do not count them.
    /// </summary>
    [DataField]
    public EntityWhitelist Blacklist = new();

    /// <summary>
    /// How many entities to select.
    /// </summary>
    [DataField]
    public int Amount = 1;
}
