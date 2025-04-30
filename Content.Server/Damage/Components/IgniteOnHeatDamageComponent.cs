// SPDX-FileCopyrightText: 2024 LankLTE <135308300+LankLTE@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.FixedPoint;

namespace Content.Server.Damage.Components;

[RegisterComponent]
public sealed partial class IgniteOnHeatDamageComponent : Component
{
    [DataField("fireStacks")]
    public float FireStacks = 1f;

    // The minimum amount of damage taken to apply fire stacks
    [DataField("threshold")]
    public FixedPoint2 Threshold = 15;
}