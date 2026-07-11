// SPDX-License-Identifier: AGPL-3.0-or-later

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