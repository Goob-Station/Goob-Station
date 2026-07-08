// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Security;

[Serializable, NetSerializable]
public sealed partial class PanicButtonDoAfterEvent : SimpleDoAfterEvent
{
}
