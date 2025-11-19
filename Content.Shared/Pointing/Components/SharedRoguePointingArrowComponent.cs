// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Pointing.Components
{
    [NetworkedComponent]
    public abstract partial class SharedRoguePointingArrowComponent : Component
    {
    }

    [Serializable, NetSerializable]
    public enum RoguePointingArrowVisuals : byte
    {
        Rotation
    }
}
