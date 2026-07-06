// SPDX-FileCopyrightText: 2026 Jonikibaka
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.MalfunctionAi;

/// <summary>
/// Marks a cyborg that has already been hacked/subverted by a Malfunction AI, so it cannot be
/// hacked again.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MalfHackedCyborgComponent : Component;
