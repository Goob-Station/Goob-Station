// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Trauma.Shared.Ranching.Events;

[Serializable, NetSerializable]
public sealed partial class FertilizeDoAfterEvent : SimpleDoAfterEvent;
