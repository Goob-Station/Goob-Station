// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Administration.Components;

/// <summary>
/// Flips the target's sprite on its head, so they do a headstand.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HeadstandComponent : Component;
