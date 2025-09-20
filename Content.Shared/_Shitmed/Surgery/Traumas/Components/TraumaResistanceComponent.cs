// SPDX-FileCopyrightText: 2025 RichardBlonski <48651647+RichardBlonski@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;

/// <summary>
/// Component that defines species-specific trauma resistances and modifiers.
/// This allows different species to have different susceptibilities to various trauma types.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TraumaResistanceComponent : Component
{
    /// <summary>
    /// Multiplier for bone damage chance.
    /// 0 = completely immune to bone damage
    /// 1 = normal bone damage chance
    /// >1 = increased bone damage chance
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 BoneDamageMultiplier { get; set; } = 1.0;

    /// <summary>
    /// Multiplier for dismemberment chance.
    /// 0 = completely immune to dismemberment
    /// 1 = normal dismemberment chance
    /// >1 = increased dismemberment chance
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 DismembermentMultiplier { get; set; } = 1.0;

    /// <summary>
    /// Multiplier for organ damage chance.
    /// 0 = completely immune to organ damage
    /// 1 = normal organ damage chance
    /// >1 = increased organ damage chance
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 OrganDamageMultiplier { get; set; } = 1.0;

    /// <summary>
    /// Multiplier for nerve damage chance.
    /// 0 = completely immune to nerve damage
    /// 1 = normal nerve damage chance
    /// >1 = increased nerve damage chance
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 NerveDamageMultiplier { get; set; } = 1.0;

    /// <summary>
    /// Maximum dismemberment chance cap for this species.
    /// This overrides the global maximum if set to a value other than -1.
    /// values other than -1 = chance aka 0.8 = 80% chance
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 MaxDismembermentChance { get; set; } = -1;

    /// <summary>
    /// Whether this species is completely immune to bone damage.
    /// This is a convenience property that sets BoneDamageMultiplier to 0.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool BoneDamageImmune { get; set; } = false;
}
