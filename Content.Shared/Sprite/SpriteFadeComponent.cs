// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.GameStates;

namespace Content.Shared.Sprite;

/// <summary>
/// If your client entity is behind this then the sprite's alpha will be lowered so your entity remains visible.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SpriteFadeComponent : Component
{

}
