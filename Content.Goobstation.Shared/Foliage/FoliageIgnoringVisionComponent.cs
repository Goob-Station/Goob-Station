// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Foliage;

/// <summary>
/// Makes "Foliage" with the IsFoliage Component render lower for the entity with this Component.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class FoliageIgnoringVisionComponent : Component;
