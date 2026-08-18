// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Hood.Clothing;

/// <summary>
/// Opt-in marker for items that remain mechanically equipped in the belt slot without drawing on the wearer.
/// In-hand and every other equipment-slot visual remain unchanged.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SuppressEquippedVisualInBeltComponent : Component;
