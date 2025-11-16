// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Enchanting.Systems;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Enchanting.Components;

/// <summary>
/// Component required for all enchants.
/// Events are relayed by <see cref="EnchantRelaySystem"/> to be used for behaviour.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(EnchantingSystem))]
[AutoGenerateComponentState]
[EntityCategory("Enchants")]
public sealed partial class EnchantComponent : Component
{
    /// <summary>
    /// Whitelist for items that this enchant can be applied to.
    /// Used to prevent picking invalid enchants on items.
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// Blacklist for items that cannot be enchanted, even if they match the whitelist.
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist;

    /// <summary>
    /// Enchants this cannot be combined with.
    /// This is one-way so this id should be on the others as well.
    /// </summary>
    [DataField]
    public List<EntProtoId<EnchantComponent>> Incompatible = new();

    /// <summary>
    /// The rolled level of this enchant.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Level = 1;

    /// <summary>
    /// The max level this enchant can be rolled at.
    /// </summary>
    [DataField]
    public int MaxLevel = 1;

    /// <summary>
    /// Whether to show the level when examining.
    /// </summary>
    public bool ShowLevel => MaxLevel > 1;

    /// <summary>
    /// Maxed enchants can't be upgraded further.
    /// </summary>
    [ViewVariables]
    public bool IsMaxed => Level == MaxLevel;

    /// <summary>
    /// The entity this enchant belongs to.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Enchanted;
}

/// <summary>
/// Event raised on enchant entities after they get added for the first time.
/// </summary>
[ByRefEvent]
public readonly record struct EnchantAddedEvent(EnchantComponent Comp, EntityUid Item)
{
    public int Level => Comp.Level;
}

/// <summary>
/// Event raised on enchant entities after they get upgraded to a new level.
/// You should the old level into account when e.g. modifying stats.
/// </summary>
[ByRefEvent]
public readonly record struct EnchantUpgradedEvent(EnchantComponent Comp, EntityUid Item, int OldLevel)
{
    public int Level => Comp.Level;
}
