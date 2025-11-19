// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.GameStates;

namespace Content.Shared.NPC;

/// <summary>
/// Added to NPCs that are actively being updated.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ActiveNPCComponent : Component {}
