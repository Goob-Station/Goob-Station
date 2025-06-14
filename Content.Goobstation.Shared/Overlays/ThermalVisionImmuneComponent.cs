// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 widgetbeck <beckparrott@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Overlays;

// imp. if an entity has this component, it will be ignored by the thermal vision overlay. this includes if it's in a container, or is itself a container.
[RegisterComponent, NetworkedComponent]
public sealed partial class ThermalVisionImmuneComponent : Component
{

}
