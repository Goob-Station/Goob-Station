// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.SlaughterDemon.Objectives;

/// <summary>
/// This is used for the objective which is for devouring everyone on the station
/// </summary>
[RegisterComponent]
public sealed partial class SlaughterKillEveryoneConditionComponent : Component
{
    [DataField]
    public string? Title;

    [DataField]
    public string? Description;

    [DataField]
    public int Devoured;
}
