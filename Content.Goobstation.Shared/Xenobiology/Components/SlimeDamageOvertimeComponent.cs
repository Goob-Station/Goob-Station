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

    // 2u of blood -> 0.2u + 1.5u = 1.7 uncooked protein -> 0.85u protein -> 2.55 hunger

    /// <summary>
    /// How many units from target's bloodstream would be sucked per tick
    /// </summary>
    [DataField]
    public FixedPoint2 SuctionUnits = 2;

    /// <summary>
    /// Which reagent will end up in the slime's stomach when eating the target
    /// </summary>
    [DataField]
    public ProtoId<ReagentPrototype> FoodReagent = "UncookedAnimalProteins";

    /// <summary>
    /// How many food units will be added to the slime's stomach when eating the target
    /// </summary>
    [DataField]
    public FixedPoint2 FoodUnits = 1.5;

    //public ProtoId<ReagentPrototype> PoisonReagent = "UncookedAnimalProteins";

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
