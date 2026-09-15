// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Oskarrr.WorkingJoe;

[Serializable, NetSerializable]
public sealed partial class WorkingJoeRebootDoAfterEvent : SimpleDoAfterEvent;
