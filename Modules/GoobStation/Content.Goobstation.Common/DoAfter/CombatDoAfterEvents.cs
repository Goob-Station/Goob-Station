// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.DoAfter;

[ByRefEvent]
public readonly record struct CombatModeToggledEvent(EntityUid User, bool Activated);

[ByRefEvent]
public readonly record struct EnsnaredEvent(EntityUid Target);
