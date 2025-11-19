// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿using Robust.Shared.Serialization;

namespace Content.Shared.Disposal.Components
{
    [Serializable, NetSerializable]
    public enum DisposalTubeVisuals
    {
        VisualState
    }

    [Serializable, NetSerializable]
    public enum DisposalTubeVisualState
    {
        Free = 0,
        Anchored,
    }
}
