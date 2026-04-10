// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DoAfter;
using Content.Shared.Emag.Systems;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Emag;

[Serializable, NetSerializable]
public sealed partial class EmergencyShuttleConsoleEmagDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class CleaningEmaggedDeviceDoAfterEvent : SimpleDoAfterEvent { }

/// <summary>
/// Raised on the entity when it's emag is cleaned 
/// </summary>
/// <param name="User">The entity</param>
/// <param name="EmagType">The type of emag that was cleaned</param>
/// <param name="Handled">If event is handled by previous system</param>
[ByRefEvent]
public record struct EmagCleanedEvent(EntityUid User, EmagType EmagType = EmagType.Jestographic, bool Handled = false);
