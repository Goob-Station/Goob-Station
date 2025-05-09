// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Lube;

[RegisterComponent]
public sealed partial class LubedComponent : Component
{
    [DataField("slipsLeft"), ViewVariables(VVAccess.ReadWrite)]
    public int SlipsLeft;

    [DataField("slipStrength"), ViewVariables(VVAccess.ReadWrite)]
    public int SlipStrength;
}