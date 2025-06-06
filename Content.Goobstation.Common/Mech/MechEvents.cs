// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Mech;

/// <summay>
/// Raised on an entity that has been inserted into a mech as a pilot.
/// </summary>
public record struct MechInsertedEvent(EntityUid mechUid);

/// <summay>
/// Raised on an entity that has been ejected from a mech as its pilot.
/// </summary>
public record struct MechEjectedEvent(EntityUid mechUid);
