// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.ItemUpgrades.Components;

[Access(typeof(ItemUpgradesSystem))]
[RegisterComponent, NetworkedComponent]
public sealed partial class ItemUpgradeComponent : Component
{
    /// <summary>
    /// Literal name of this upgrade that is shown on all examine texts.
    /// </summary>
    [DataField(required: true)]
    public LocId Name;

    /// <summary>
    /// Text to use when examining the upgrade itself.
    /// </summary>
    [DataField]
    public LocId? ExamineTextType = "gun-upgrade-examine-type-upgrade";

    /// <summary>
    /// Text template to use when examining the parent where this upgrade is inserted to.
    /// </summary>
    [DataField]
    public LocId? InsertedTextType = "gun-upgrade-inserted-examine-type-contains";

    /// <summary>
    /// If this string matches with some other item upgrade, it will fail to install.
    /// </summary>
    [DataField]
    public string? UniqueGroup;
}
