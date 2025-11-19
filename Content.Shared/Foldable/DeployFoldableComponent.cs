// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿using Robust.Shared.GameStates;

namespace Content.Shared.Foldable;

[RegisterComponent, NetworkedComponent]
[Access(typeof(DeployFoldableSystem))]
public sealed partial class DeployFoldableComponent : Component;
