// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Veritius <veritiusgaming@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Pointing.Components;

/// <summary>
/// Causes pointing arrows to go mode and murder this entity.
/// </summary>
[RegisterComponent]
public sealed partial class PointingArrowAngeringComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("remainingAnger")]
    public int RemainingAnger = 5;
}