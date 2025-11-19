// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿using Robust.Shared.Serialization;

namespace Content.Shared.Trigger;

[Serializable, NetSerializable]
public enum ProximityTriggerVisuals : byte
{
    Off,
    Inactive,
    Active,
}

[Serializable, NetSerializable]
public enum ProximityTriggerVisualState : byte
{
    State,
}

[Serializable, NetSerializable]
public enum TriggerVisuals : byte
{
    VisualState,
}

[Serializable, NetSerializable]
public enum TriggerVisualState : byte
{
    Primed,
    Unprimed,
}
