// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.MantisBlades;

/// <summary>
/// Marker comp for mantis blades. This will also be required later for the cyberware UI.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MantisBladeArmComponent : Component;
