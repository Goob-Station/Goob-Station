// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Magnus Larsen <i.am.larsenml@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Client.Weapons.Melee.Components;

/// <summary>
/// Used for melee attack animations. Typically just has a fadeout.
/// </summary>
[RegisterComponent]
public sealed partial class WeaponArcVisualsComponent : Component
{
    public EntityUid? User;

    [DataField("animation")]
    public WeaponArcAnimation Animation = WeaponArcAnimation.None;

    [ViewVariables(VVAccess.ReadWrite), DataField("fadeOut")]
    public bool Fadeout = true;
}

public enum WeaponArcAnimation : byte
{
    None,
    Thrust,
    Slash,
}