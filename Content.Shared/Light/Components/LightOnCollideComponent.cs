// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
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