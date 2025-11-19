// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Spillable;

[Serializable, NetSerializable]
public sealed partial class SpillDoAfterEvent : SimpleDoAfterEvent
{
}
