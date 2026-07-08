// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.GPS.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class HandheldGPSComponent : Component
{
    [DataField]
    public float UpdateRate = 1.5f;
}