// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Fluids;

public abstract partial class SharedDrainSystem : EntitySystem
{
    [Serializable, NetSerializable]
    public sealed partial class DrainDoAfterEvent : SimpleDoAfterEvent
    {
    }
}
