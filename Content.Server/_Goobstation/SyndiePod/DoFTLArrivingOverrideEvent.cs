// SPDX-FileCopyrightText: 2025 FaDeOkno <logkedr18@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Goobstation.DropPod;

[ByRefEvent]
public record struct DoFTLArrivingOverrideEvent(bool Handled = false, bool PlaySound = true, bool Cancelled = false);
