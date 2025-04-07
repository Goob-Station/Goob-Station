// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
namespace Content.Server.Heretic.Components.PathSpecific;

[RegisterComponent]
public sealed partial class RiposteeComponent : Component
{
    [DataField] public float Cooldown = 20f;
    [ViewVariables(VVAccess.ReadWrite)] public float Timer = 20f;

    [DataField] public bool CanRiposte = true;
}