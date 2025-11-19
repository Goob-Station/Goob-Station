// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.Serialization;

namespace Content.Shared.Singularity.Components
{
    [NetSerializable, Serializable]
    public enum RadiationCollectorVisuals
    {
        VisualState,
        TankInserted,
        PressureState,
    }

    [NetSerializable, Serializable]
    public enum RadiationCollectorVisualState
    {
        Active = (1<<0),
        Activating = (1<<1) | Active,
        Deactivating = (1<<1),
        Deactive = 0
    }
}
