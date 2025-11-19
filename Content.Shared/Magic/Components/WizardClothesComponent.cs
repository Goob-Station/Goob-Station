// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿using Robust.Shared.GameStates;

namespace Content.Shared.Magic.Components;

/// <summary>
/// The <see cref="SharedMagicSystem"/> checks this if a spell requires wizard clothes
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedMagicSystem))]
public sealed partial class WizardClothesComponent : Component;
