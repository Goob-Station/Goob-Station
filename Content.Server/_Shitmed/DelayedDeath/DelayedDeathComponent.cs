// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Shitmed.DelayedDeath;

[RegisterComponent]
public sealed partial class DelayedDeathComponent : Component
{
    /// <summary>
    /// How long it takes to kill the entity.
    /// </summary>
    [DataField]
    public float DeathTime = 60;

    /// <summary>
    /// How long it has been since the delayed death timer started.
    /// </summary>
    public float DeathTimer;

    /// <summary>
    /// What message is displayed when the time runs out - Goobstation
    /// </summary>
    [DataField]
    public LocId DeathMessageId;

    /// <summary>
    /// What the defib displays when attempting to revive this entity. - Goobstation
    /// </summary>
    [DataField]
    public LocId DefibFailMessageId = "defibrillator-missing-organs";
}
