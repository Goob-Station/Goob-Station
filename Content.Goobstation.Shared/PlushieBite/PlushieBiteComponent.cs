// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Baptr0b0t
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Components;
using Content.Shared.Damage;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.PlushieBite;

/// <summary>
/// Make holder have a periodic chance to be bitten.
/// </summary>
[RegisterComponent]
public sealed partial class PlushieBiteComponent : Component
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
    /// Sound played on bite.
    /// </summary>
    [DataField]
    public SoundSpecifier? BiteSound;

    /// <summary>
    /// Reagents injected into the holder's bloodstream on each bite.
    /// </summary>
    [DataField]
    public Solution? BiteReagents;

    [ViewVariables]
    public EntityUid? Holder;

    [ViewVariables]
    public TimeSpan NextBiteTime;
}
