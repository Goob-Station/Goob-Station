// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Flash.Components;

/// <summary>
/// Exists for use as a status effect. Adds a shader to the client that obstructs vision.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class FlashedComponent : Component;
