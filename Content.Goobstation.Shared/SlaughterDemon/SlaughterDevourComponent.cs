// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.SlaughterDemon;

/// <summary>
/// This is used for marking an entity as able to devour people with blood crawl
/// </summary>
[RegisterComponent]
public sealed partial class SlaughterDevourComponent : Component
{
    /// <summary>
    /// Healing done when eating someone
    /// </summary>
    [DataField]
    public DamageSpecifier ToHeal = new()
    {
        DamageDict = new()
        {
            { "Brute", -1000 },
            { "Blunt", -1000 },
            { "Slash", -1000 },
            { "Piercing", -1000 },
            { "Heat", -1000 },
            { "Cold", -1000 },
            { "Shock", -1000 },
        }
    };

    /// <summary>
    /// Healing done when eating a robot
    /// </summary>
    [DataField]
    public DamageSpecifier ToHealNonCrew = new()
    {
        DamageDict = new()
        {
            { "Brute", -50 },
            { "Blunt", -50 },
            { "Slash", -50 },
            { "Piercing", -50 },
            { "Heat", -50 },
            { "Cold", -50 },
            { "Shock", -50 },
        }
    };

    /// <summary>
    /// Healing done when eating anything else
    /// </summary>
    [DataField]
    public DamageSpecifier ToHealAnythingElse = new()
    {
        DamageDict = new()
        {
            { "Brute", -25 },
            { "Blunt", -25 },
            { "Slash", -25 },
            { "Piercing", -25 },
            { "Heat", -25 },
            { "Cold", -25 },
            { "Shock", -25 },
        }
    };

    /// <summary>
    /// The sound that plays once devouring someone
    /// </summary>
    [DataField]
    public SoundSpecifier? FeastSound = new SoundPathSpecifier("/Audio/Effects/demon_consume.ogg")
    {
        Params = AudioParams.Default.WithVolume(-3f),
    };

    /// <summary>
    /// Laughter Demon exclusive: A container that holds the entities instead of outright removing them
    /// </summary>
    public Container Container = default!;
}
