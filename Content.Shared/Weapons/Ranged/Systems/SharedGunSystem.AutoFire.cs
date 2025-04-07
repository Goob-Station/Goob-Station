// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT
using Content.Shared.Weapons.Ranged.Components;

namespace Content.Shared.Weapons.Ranged.Systems;

public partial class SharedGunSystem
{
    public void SetEnabled(EntityUid uid, AutoShootGunComponent component, bool status)
    {
        component.Enabled = status;
    }
}