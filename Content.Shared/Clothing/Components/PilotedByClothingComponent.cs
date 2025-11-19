// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Content.Shared.Clothing.EntitySystems;
using Robust.Shared.GameStates;

namespace Content.Shared.Clothing.Components;

/// <summary>
/// Disables client-side physics prediction for this entity.
/// Without this, movement with <see cref="PilotedClothingSystem"/> is very rubberbandy.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PilotedByClothingComponent : Component
{
}
