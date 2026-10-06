// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Shitmed.Damage;

/// <summary>
/// Determines how dealt damage is split upon a body.
/// </summary>
public enum SplitDamageBehavior
{
    /// <summary>
    /// No damage split.
    /// </summary>
    None,
    /// <summary>
    /// The damage will be split on all of the body parts.
    /// </summary>
    Split,
    /// <summary>
    /// The damage will be split upon all of the vital parts, for explosions obviously.
    /// </summary>
    SplitExplosion,
    /// <summary>
    /// The damage will be split upon all already-damaged body parts, then SplitEnsureAll behavior.
    /// </summary>
    SplitEnsureAllDamaged,
    /// <summary>
    /// The damage will be split upon all organic body parts, then SplitEnsureAll behavior.
    /// </summary>
    SplitEnsureAllOrganic,
    /// <summary>
    /// The damage will be split upon all already-damaged & organic body parts, then SplitEnsureAll behavior.
    /// </summary>
    SplitEnsureAllDamagedAndOrganic,
    /// <summary>
    /// The damage will be split on all of the body parts, but Quirky??
    /// </summary>
    SplitEnsureAll
}
