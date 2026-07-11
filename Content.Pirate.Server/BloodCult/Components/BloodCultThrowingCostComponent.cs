// SPDX-FileCopyrightText: 2024 White Dream Project contributors
// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server.BloodCult.Components;

[RegisterComponent]
public sealed partial class BloodCultThrowingCostComponent : Component
{
    [DataField]
    public float StaminaCost = 8f;
}
