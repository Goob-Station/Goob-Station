// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Xenobiology.Components;

/// <summary>
/// This is used for slime latching damage, this can be expanded in the future to allow for special breed dependent effects.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SlimeDamageOvertimeComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? SourceEntityUid;

    /// <summary>
    /// How many units from target's bloodstream would be sucked per tick
    /// </summary>
    [DataField]
    public FixedPoint2 SuctionUnits = 2.5; // just enough to suck all blood from monkey in 1 latch

    /// <summary>
    /// What toxin would be injected inside target's bloodstream
    /// </summary>
    [DataField]
    public ProtoId<ReagentPrototype> ToxinReagent = "XenobioSlimeToxin";

    /// <summary>
    /// How many toxin units will be added to the targets bloodstream when eating the target
    /// </summary>
    [DataField]
    public FixedPoint2 ToxinUnits = 0.15;

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
