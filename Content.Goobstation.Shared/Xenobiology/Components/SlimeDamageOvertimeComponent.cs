// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Xenobiology.Components;

/// <summary>
/// This is used for slime latching damage, this can be expanded in the future to allow for special breed dependent effects.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SlimeDamageOvertimeComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? SourceEntityUid;

    [DataField]
    public TimeSpan Interval = TimeSpan.FromSeconds(1);

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan NextTickTime = TimeSpan.Zero;

    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Caustic", 2.5 },
        },
    };
}
