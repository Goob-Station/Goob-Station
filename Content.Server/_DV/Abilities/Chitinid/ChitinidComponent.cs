// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ark <189933909+ark1368@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._DV.Abilities.Chitinid;

/// <summary>
/// Passively heals radiation up to a limit, which then uses <c>ItemCougherComponent</c> to cough up Chitzite.
/// After that it will heal radiation damage again.
/// </summary>
[RegisterComponent, Access(typeof(ChitinidSystem))]
[AutoGenerateComponentPause]
public sealed partial class ChitinidComponent : Component
{
    [DataField]
    public FixedPoint2 AmountAbsorbed = 0f;

    /// <summary>
    /// Once this much damage is absorbed, it will stop healing and require you to cough up chitzite.
    /// </summary>
    [DataField]
    public FixedPoint2 MaximumAbsorbed = 30f;

    /// <summary>
    /// What damage is healed, by adding, every <see cref="UpdateInterval"/>.
    /// This must be negative.
    /// </summary>
    [DataField]
    public DamageSpecifier Healing = new()
    {
        DamageDict = new()
        {
            { "Radiation", -0.5 },
        }
    };

    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan NextUpdate;
}