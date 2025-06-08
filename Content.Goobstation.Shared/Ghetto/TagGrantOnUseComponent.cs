// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferEOS <stepanteliatnik2022@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Ghetto;

/// <summary>
/// Grants a specific tag to a user when activated.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TagGrantOnUseComponent : Component
{
    /// <summary>
    /// Number of times this item can be used
    /// Null indicates infinite uses.
    /// </summary>
    [DataField]
    public int? Uses = 1;

    /// <summary>
    /// Required tag to grant when used
    /// </summary>
    [DataField(required: true)]
    public ProtoId<TagPrototype> Tag = default!;

    /// <summary>
    /// Localization string for the popup message when used.
    /// Null indicates no popup should be shown.
    /// </summary>
    [DataField]
    public LocId? Popup;
}
