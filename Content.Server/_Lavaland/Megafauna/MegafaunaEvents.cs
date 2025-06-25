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

namespace Content.Server._Lavaland.Megafauna;

/// <summary>
/// Raised when boss is fully defeated.
/// </summary>
public sealed class MegafaunaKilledEvent : EntityEventArgs;

/// <summary>
/// Raised when boss starts proceeding it's logic.
/// </summary>
public sealed class MegafaunaStartupEvent : EntityEventArgs;

/// <summary>
/// Raised when boss doesn't die but for any reason deactivates.
/// </summary>
public sealed class MegafaunaShutdownEvent : EntityEventArgs;

/// <summary>
/// Event that is raised on some aggressor target, for Complex megafauna AI
/// to analyze and change its attacks weights to adjust to player's skills or power.
/// It's like simple neural network, but you hardcode its connections by yourself.
/// </summary>
[ByRefEvent]
public record struct MegafaunaComplexCheckEvent(
    // Treat this thing like a neural network, so remember 3 things:
    // 1. Everything should be in Float type in 0-1 range
    // 2. Specify only player's data
    // 3. Sort everything in alphabetical order

    float TargetHp, // Percent of HP until crit, equals to 1 if already in crit and 0 on full health
    float TargetArmor, // Average resistance against megafaunas types of damage
    float TargetDistance, // Distance in 0-10 tiles range (percent)
    float WeaponType // Currently held weapon: 0 if melee, 1 if ranged
    );
