// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class RiposteeComponent : Component
{
    [DataField]
    public float Cooldown = 20f;

    [DataField]
    public EntityWhitelist? WeaponWhitelist = new()
    {
        Tags = new()
        {
            "HereticBlade",
        },
    };

    [ViewVariables(VVAccess.ReadWrite)]
    public float Timer = 20f;

    [DataField]
    public bool CanRiposte = true;
}
