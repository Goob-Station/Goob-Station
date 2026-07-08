// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Overlays;

[RegisterComponent, NetworkedComponent]
public sealed partial class ThermalVisionComponent : SwitchableVisionOverlayComponent
{
    public override EntProtoId? ToggleAction { get; set; } = "ToggleThermalVision";

    public override Color Color { get; set; } = Color.FromHex("#d06764");

    [DataField]
    public float LightRadius = 2f;

    [DataField]
    public string? ThermalShader = "ThermalVision";
}

public sealed partial class ToggleThermalVisionEvent : InstantActionEvent;
