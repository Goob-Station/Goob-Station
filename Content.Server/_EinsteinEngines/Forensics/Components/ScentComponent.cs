// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._EinsteinEngines.Forensics.Components;

/// <summary>
/// This component is for mobs that have a Scent.
/// </summary>
[RegisterComponent]
public sealed partial class ScentComponent : Component
{
    [DataField]
    public string Scent = string.Empty;
}
