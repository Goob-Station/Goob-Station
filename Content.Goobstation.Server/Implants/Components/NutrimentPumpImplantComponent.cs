// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.Implants.Components;

[RegisterComponent]
public sealed partial class NutrimentPumpImplantComponent : Component
{
    /// <summary>
    /// Did the entity have thirst before being implanted?
    /// </summary>
    [DataField] public bool HadThirst = false;

    /// <summary>
    /// Did the entity have hunger before being implanted?
    /// </summary>
    [DataField] public bool HadHunger = false;
}
