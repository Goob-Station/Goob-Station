// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Throwing;

/// <summary>
/// Raised on an entity after it has thrown something.
/// </summary>
[ByRefEvent]
public readonly record struct ThrowEvent(EntityUid? User, EntityUid Thrown);

/// <summary>
/// Raised directed on the target entity being hit by the thrown entity.
/// </summary>
[ByRefEvent]
public readonly record struct ThrowHitByEvent(EntityUid Thrown, EntityUid Target, ThrownItemComponent Component);

/// <summary>
/// Raised directed on the thrown entity that hits another.
/// </summary>
[ByRefEvent]
public readonly record struct ThrowDoHitEvent(EntityUid Thrown, EntityUid Target, ThrownItemComponent Component);
