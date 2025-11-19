// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿using Robust.Shared.Serialization;

namespace Content.Shared.Destructible;

[Flags]
[Serializable, NetSerializable]
public enum ThresholdActs : byte
{
    None = 0,
    Breakage = 1 << 0,
    Destruction = 1 << 1,
}
