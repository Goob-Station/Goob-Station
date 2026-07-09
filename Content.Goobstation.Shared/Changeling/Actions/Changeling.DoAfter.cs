// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Changeling.Actions;

[Serializable, NetSerializable]
public sealed partial class AbsorbDNADoAfterEvent : SimpleDoAfterEvent { }
[Serializable, NetSerializable]
public sealed partial class AbsorbBiomatterDoAfterEvent : SimpleDoAfterEvent { }