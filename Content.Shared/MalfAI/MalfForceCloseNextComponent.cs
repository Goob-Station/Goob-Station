// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Shared.MalfAI;

/// <summary>
/// Marker component used by the Malf AI lockdown to force the next door close
/// attempt to ignore collision checks (i.e., behave as if safety was disabled).
/// It is removed as soon as the door begins closing.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MalfForceCloseNextComponent : Component
{
}
