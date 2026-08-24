// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Light.Components;

/// <summary>
/// Enables / disables pointlight whenever entities are contacting with it
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class LightOnCollideComponent : Component
{
}