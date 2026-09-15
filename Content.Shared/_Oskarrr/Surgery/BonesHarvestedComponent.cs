// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Oskarrr.Surgery;

/// <summary>
/// Marks a body part whose anatomical bones were surgically extracted (not naturally boneless).
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BonesHarvestedComponent : Component;
