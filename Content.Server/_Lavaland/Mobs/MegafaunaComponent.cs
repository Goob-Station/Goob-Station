// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aineias1 <dmitri.s.kiselev@gmail.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 McBosserson <148172569+McBosserson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <plmilonpl@gmail.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
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

using Robust.Shared.Prototypes;
using System.Threading;

namespace Content.Server._Lavaland.Mobs;

[Virtual, RegisterComponent]
public partial class MegafaunaComponent : Component
{
    /// <summary>
    ///     Used for all the timers that get assigned to the boss.
    ///     In theory all bosses should use it so i'll just leave it here.
    /// </summary>
    [NonSerialized] public CancellationTokenSource CancelToken = new();

    /// <summary>
    ///     Whether or not it should power trip aggressors or random locals
    /// </summary>
    [DataField] public bool Aggressive = false;

    /// <summary>
    ///     Should it drop guaranteed loot when dead? If so what exactly?
    /// </summary>
    [DataField] public EntProtoId? Loot = null;

    /// <summary>
    ///     Should it drop something besides the main loot as a crusher only reward?
    /// </summary>
    [DataField] public EntProtoId? CrusherLoot = null;

    /// <summary>
    ///     Check if the boss got damaged by crusher only.
    ///     True by default. Will immediately switch to false if anything else hit it. Even the environmental stuff.
    /// </summary>
    [DataField] public bool CrusherOnly = true;
}