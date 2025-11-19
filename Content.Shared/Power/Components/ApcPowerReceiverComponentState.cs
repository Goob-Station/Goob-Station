// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿using Robust.Shared.Serialization;

namespace Content.Shared.Power.Components;

[Serializable, NetSerializable]
public sealed class ApcPowerReceiverComponentState : ComponentState
{
    public bool Powered;
    public bool NeedsPower;
    public bool PowerDisabled;
}
