// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: MIT

using Content.Shared.CombatMode;
using Content.Shared.Roles;

namespace Content.Goobstation.Shared.MisandryBox.PlaytimeDegrade;

/// <summary>
/// Degrades mob stats and movement speed depending on playtime of that particular job.
/// </summary>
public sealed partial class PlaytimeDegrade : JobSpecial
{
    /// <summary>
    /// Start decaying stats after playing this amount of minutes
    /// </summary>
    [DataField(required: true)]
    public int Since { get; set; }

    /// <summary>
    /// How much to decay by for every minute of playtime beyond Since
    /// </summary>
    [DataField]
    public float? By { get; set; } = null;

    /// <summary>
    /// Start decaying until this amount of playtime reached. This will be the floor for stats.
    /// </summary>
    [DataField(required: true)]
    public int Until { get; set; }

    /// <summary>
    /// Floor for the decay, in percent. By default 25%
    /// </summary>
    [DataField]
    public float Floor { get; set; } = 0.25f;

    /// <summary>
    /// Apply <see cref="DisarmMalusComponent"/> at halfway point?
    /// </summary>
    [DataField]
    public bool DisarmMalus { get; set; } = false;

    public override void AfterEquip(EntityUid mob)
    {
        var entManager = IoCManager.Resolve<IEntityManager>();

        entManager.EnsureComponent<PlaytimeDegradeComponent>(mob, out var comp);
        comp.Since = Since;
        comp.By = By;
        comp.Until = Until;
        comp.Floor = Floor;
        comp.DisarmMalus = DisarmMalus;
    }
}
