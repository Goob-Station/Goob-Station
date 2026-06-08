// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Baptr0b0t 
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.SpeciesAffinity;

/// <summary>
/// Holders whose species is not in <see cref="FavoredSpecies"/> have a periodic chance to be bitten.
/// </summary>
[RegisterComponent]
public sealed partial class SpeciesAffinityComponent : Component
{
    /// <summary>
    /// Any species not in this list is considered unfavored and may get bitten.
    /// </summary>
    [DataField(required: true)]
    public List<ProtoId<SpeciesPrototype>> FavoredSpecies = [];

    /// <summary>
    /// Probability per interval that an unfavored holder gets bitten.
    /// </summary>
    [DataField]
    public float BiteChance = 0.15f;

    /// <summary>
    /// Time between bite checks for unfavored holders.
    /// </summary>
    [DataField]
    public TimeSpan BiteInterval = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Damage dealt to an unfavored holder per bite.
    /// </summary>
    [DataField]
    public DamageSpecifier BiteDamage = new();

    /// <summary>
    /// Sounds played on bite. A random one is picked each time. No sound plays if empty.
    /// </summary>
    [DataField]
    public List<string> BiteSounds = [];

    [ViewVariables]
    public EntityUid? Holder;
    
    [ViewVariables]
    public bool IsHolderFavored;
    
    [ViewVariables]
    public TimeSpan NextBiteTime;
}
