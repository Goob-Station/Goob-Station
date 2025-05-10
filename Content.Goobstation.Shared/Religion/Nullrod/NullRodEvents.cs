// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Religion.Nullrod.Components;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Religion.Nullrod;

/// <summary>
/// 	Raised on the nullrod when praying.
/// </summary>
/// <param name="User">The entity praying at the nullrod.</param>
[ByRefEvent]
public record struct AlternatePrayEvent(EntityUid User);

[Serializable, NetSerializable]
public sealed partial class AlternatePrayDoAfterEvent : SimpleDoAfterEvent;
