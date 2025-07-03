// SPDX-FileCopyrightText: 2025 BeBright <98597725+be1bright@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Silicons.Borgs;

/// <summary>
///  Information relating to a borg's subtype. Should be purely cosmetic.
/// </summary>
[Prototype]
public sealed partial class BorgSubtypePrototype : IPrototype
{
    [IdDataField]
    public required string ID { get; set; }

    /// <summary>
    /// Prototype to display in the selection menu for the subtype.
    /// </summary>
    [DataField]
    public required EntProtoId DummyPrototype;

    /// <summary>
    /// The sprite path belonging to this particular subtype.
    /// </summary>
    [DataField]
    public required ResPath SpritePath;

    /// <summary>
    /// The parent borg type that the subtype will be shown under in the selection menu.
    /// </summary>
    [DataField]
    public required ProtoId<BorgTypePrototype> ParentBorgType = "generic";
}
