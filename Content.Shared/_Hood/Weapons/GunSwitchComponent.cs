// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Hood.Weapons;

/// <summary>
/// Marks the fictional fire-control switch accepted by compatible Hood firearms.
/// The item is deliberately an abstract gameplay attachment, not a real conversion part.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class GunSwitchComponent : Component;
