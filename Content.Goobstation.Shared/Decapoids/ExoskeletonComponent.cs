// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Trauma.Shared.Decapoids;

/// <summary>
/// Prevents the entity from being injected with syringes altogether.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ExoskeletonComponent : Component;
