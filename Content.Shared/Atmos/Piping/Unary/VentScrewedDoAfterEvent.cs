// SPDX-FileCopyrightText: 2024 PotentiallyTom <67602105+PotentiallyTom@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Atmos.Piping.Unary;

[Serializable, NetSerializable]
public sealed partial class VentScrewedDoAfterEvent : SimpleDoAfterEvent
{
}