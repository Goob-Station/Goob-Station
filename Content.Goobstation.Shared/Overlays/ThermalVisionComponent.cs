// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Armok <155400926+ARMOKS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Overlays;

[RegisterComponent, NetworkedComponent]
public sealed partial class ThermalVisionComponent : SwitchableVisionOverlayComponent
{
    public override EntProtoId? ToggleAction { get; set; } = "ToggleThermalVision";

    public override Color Color { get; set; } = Color.FromHex("#d9afad");

    [DataField]
    public float LightRadius = 2f;
}

public sealed partial class ToggleThermalVisionEvent : InstantActionEvent;
