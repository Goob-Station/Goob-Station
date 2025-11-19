// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.GameStates;

namespace Content.Shared.Radio.Components;

/// <summary>
/// This is used for a radio that doesn't need a telecom server in order to broadcast.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TelecomExemptComponent : Component;
