// SPDX-FileCopyrightText: 2025 BombasterDS <deniskaporoshok@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 Timfa <timfalken@hotmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage;
using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.Silicon.Bots;

/// <summary>
/// Used by the server for NPC Weldbot welding.
/// Currently no clientside prediction done, only exists in shared for emag handling.
/// </summary>
[RegisterComponent]
[Access(typeof(WeldbotSystem))]
public sealed partial class WeldbotComponent : Component
{
    /// <summary>
    /// Sound played after welding a patient.
    /// </summary>
    [DataField]
    public SoundSpecifier WeldSound = new SoundPathSpecifier("/Audio/Items/welder2.ogg");

    /// <summary>
    /// Heal or emagged damage amount
    /// </summary>
    [DataField]
    public DamageSpecifier DamageAmount = new()
    {
        DamageDict = new()
        {
            { "Blunt", -10 },
            { "Slash", -10 },
            { "Piercing", -10 },
            { "Structural", -20 }
        }
    };

    [DataField]
    public SoundSpecifier EmagSparkSound = new SoundCollectionSpecifier("sparks")
    {
        Params = AudioParams.Default.WithVolume(8f)
    };

    public bool IsEmagged = false;
}
