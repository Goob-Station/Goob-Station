// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿using Robust.Shared.GameStates;

namespace Content.Shared.Ghost;

[RegisterComponent, NetworkedComponent]
public sealed partial class GhostOnMoveComponent : Component
{
    [DataField]
    public bool CanReturn = true;

    [DataField]
    public bool MustBeDead;
}
