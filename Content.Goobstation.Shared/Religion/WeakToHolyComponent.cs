// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 mikusssssss <153551970+mikusssssss@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Religion;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class WeakToHolyComponent : Component
{
    /// <summary>
    /// Should this entity take holy damage no matter what?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool AlwaysTakeHoly;

    /// <summary>
    /// Is the entity currently standing on a rune?
    /// </summary>
    [ViewVariables]
    public bool IsColliding;

    /// <summary>
    /// Duration between each heal tick.
    /// </summary>
    [DataField]
    public TimeSpan HealTickDelay = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Used for passive healing.
    /// </summary>
    [ViewVariables]
    public TimeSpan NextPassiveHealTick;

    /// <summary>
    /// Used for rune healing.
    /// </summary>
    [ViewVariables]
    public TimeSpan NextSpecialHealTick;

    /// <summary>
    /// How much the entity is healed by runes each tick.
    /// </summary>
    [DataField]
    public DamageSpecifier HealAmount = new()
    {
        DamageDict =
        {
            ["Holy"] = -4,
        },
    };

    /// <summary>
    /// How much the entity is healed passively by each tick.
    /// </summary>
    [DataField]
    public DamageSpecifier PassiveAmount = new()
    {
        DamageDict =
        {
            ["Holy"] = -0.06,
        },
    };
}
