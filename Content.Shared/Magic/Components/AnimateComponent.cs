// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Magic.Components;

// Added to objects when they are made animate
[RegisterComponent, NetworkedComponent]
public sealed partial class AnimateComponent : Component;