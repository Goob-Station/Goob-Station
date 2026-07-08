// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Xenobiology.Components;

/// <summary>
/// Marks an entity as being consumed so it is not targeted by other entities.
/// Freaky.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BeingLatchedComponent : Component;
