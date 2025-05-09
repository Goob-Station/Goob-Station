// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
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
    [DataField]
    public int ReviveAmount = 1;

    /// <summary>
    /// Self-explanatory.
    /// </summary>
    [DataField]
    public bool InfiniteRevives;

    /// <summary>
    /// Can this entity heal themselves while not being dead?
    /// </summary>
    [DataField]
    public bool CanCheatStanding;

    [DataField]
    public EntProtoId ActionCheatDeath = "ActionCheatDeath";

    [DataField]
    public EntityUid? ActionEntity;

}

public sealed partial class CheatDeathEvent : InstantActionEvent { }
