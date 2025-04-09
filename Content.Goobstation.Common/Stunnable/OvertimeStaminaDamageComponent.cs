// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.ViewVariables;

namespace Content.Goobstation.Common.Stunnable;

[RegisterComponent, NetworkedComponent]
public sealed partial class OvertimeStaminaDamageComponent : Component
{
    [DataField] public float Delay = 1f;
    [ViewVariables(VVAccess.ReadWrite)] public float Timer = 1f;

    /// <summary>
    ///     Total amount of stamina damage a person is about to get
    /// </summary>
    [DataField] public float Amount = 10f;

    [ViewVariables(VVAccess.ReadWrite)] public float Damage = 10f;


    /// <summary>
    ///     Divisor. How much damage should we add overtime.
    /// </summary>
    /// <remarks> For example, if the divisor is 5, out entity will get the entire overtime stam damage only after 5 seconds. </remarks>
    [DataField] public float Delta = 5f;
}