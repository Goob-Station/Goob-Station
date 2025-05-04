// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.CheatDeath;

[RegisterComponent, NetworkedComponent]
public sealed partial class CheatDeathComponent : Component
{
    /// <summary>
    /// How many revives does this entity have remaining.
    /// </summary>
    /// <remarks>
    /// If set to -1, the amount is infinite.
    /// </remarks>
    [DataField]
    public int ReviveAmount = 1;

    /// <summary>
    /// Can this entity heal themselves while not being dead?
    /// </summary>
    [DataField]
    public bool CanCheatStanding;

    [DataField]
    public EntProtoId ActionCheatDeath = "ActionCheatDeath";

}

public sealed partial class CheatDeathEvent : InstantActionEvent { }
