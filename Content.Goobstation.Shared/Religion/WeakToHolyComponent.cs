// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Religion;

[RegisterComponent, NetworkedComponent]
public sealed partial class WeakToHolyComponent : Component
{
    /// <summary>
    /// Is the entity currently standing on a rune?
    /// </summary>
    [ViewVariables]
    public bool IsColliding;

    /// <summary>
    /// Duration between each heal tick while standing on a rune.
    /// </summary>
    [DataField]
    public TimeSpan HealTickDelay = TimeSpan.FromSeconds(2);

    [DataField]
    public TimeSpan NextHealTick;

    /// <summary>
    /// How much the entity is healed by each tick.
    /// </summary>
    [DataField]
    public DamageSpecifier HealAmount = new() {DamageDict = new Dictionary<string, FixedPoint2> {{ "Holy", -4 }}};

    /// <summary>
    /// The biological container.
    /// </summary>
    [ViewVariables]
    public readonly string BiologicalContainerId = "Biological";

    /// <summary>
    /// The biological metaphysical container.
    /// </summary>
    [ViewVariables]
    public readonly string MetaphysicalContainerId = "BiologicalMetaphysical";
}
