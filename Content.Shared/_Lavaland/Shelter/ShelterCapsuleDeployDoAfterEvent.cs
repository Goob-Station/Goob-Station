// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Lavaland.Shelter;

[Serializable, NetSerializable]
public sealed partial class ShelterCapsuleDeployDoAfterEvent : SimpleDoAfterEvent;