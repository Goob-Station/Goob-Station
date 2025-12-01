// SPDX-FileCopyrightText: 2025 AstroDogeDX <48888500+AstroDogeDX@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Feroxi;

/// <summary>
/// Component that allows the switching between <see cref="MetabolizerTypePrototype"/>s based on thirst
/// </summary>
[RegisterComponent, Access(typeof(FeroxiDehydrateSystem))]
public sealed partial class FeroxiDehydrateComponent : Component
{
    /// <summary>
    /// Defines which <see cref="MetabolizerTypePrototype"> to use when over the <see cref="DehydrationThreshold"/>
    /// </summary>
    [DataField(required: true)]
    public ProtoId<MetabolizerTypePrototype> HydratedMetabolizer;

    /// <summary>
    /// Defines which <see cref="MetabolizerTypePrototype"=> to use when below the <see cref="DehydrationThreshold"/>
    /// </summary>
    [DataField(required: true)]
    public ProtoId<MetabolizerTypePrototype> DehydratedMetabolizer;

    [DataField]
    public bool Dehydrated;

    /// <summary>
    /// Gives the threshold on when to flip between <see cref="HydratedMetabolizer"/> and <see cref="DehydratedMetabolizer"/>
    /// </summary>
    [DataField(required: true)]
    public float DehydrationThreshold;
}
