// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aineias1 <dmitri.s.kiselev@gmail.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 McBosserson <148172569+McBosserson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <plmilonpl@gmail.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Unlumination <144041835+Unlumy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Server._Lavaland.Mobs;

/// <summary>
/// Drops some loot when boss having this component dies.
/// </summary>
[RegisterComponent]
public sealed partial class SpawnLootOnDeathComponent : Component
{
    /// <summary>
    /// Should it drop guaranteed loot when dead? If so what exactly?
    /// </summary>
    [DataField]
    public EntProtoId? Loot;

    /// <summary>
    /// Should it drop something besides the main loot as a crusher only reward?
    /// </summary>
    [DataField]
    public EntProtoId? SpecialLoot;

    /// <summary>
    /// Whitelist for a weapon that is always checked when hitting the target.
    /// If target was damaged by something that doesn't pass this whitelist,
    /// the mob doesn't drop special loot and fallbacks to normal loot instead.
    /// </summary>
    [DataField("weaponWhitelist")]
    public EntityWhitelist? SpecialWeaponWhitelist; // MegafaunaWeaponLooter

    /// <summary>
    /// Should it delete itself after being killed?
    /// </summary>
    [DataField]
    public bool DeleteOnDeath;

    /// <summary>
    /// If true and the mob was killed with special weapon,
    /// and both loots are not null, drops both loots at once.
    /// </summary>
    [DataField]
    public bool DropBoth;

    /// <summary>
    /// Check if the boss got damaged by crusher only.
    /// True by default. Will immediately switch to false if anything else hit it. Even the environmental stuff.
    /// </summary>
    [ViewVariables]
    public bool DoSpecialLoot = true;
}
