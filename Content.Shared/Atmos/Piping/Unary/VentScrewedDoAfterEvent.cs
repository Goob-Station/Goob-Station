// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Atmos.Piping.Unary;

[Serializable, NetSerializable]
public sealed partial class VentScrewedDoAfterEvent : SimpleDoAfterEvent
{
}