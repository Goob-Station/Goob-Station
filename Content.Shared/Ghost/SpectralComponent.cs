// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Ghost;

/// <summary>
/// Marker component to identify "ghostly" entities.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SpectralComponent : Component { }