// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.MisandryBox;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.MisandryBox;

public sealed partial class AddComponentSpecial : MarkingSpecial
{
    [DataField(required: true)]
    public ComponentRegistry Components { get; private set; } = new();

    /// <summary>
    /// If this is true then existing components will be removed and replaced with these ones.
    /// </summary>
    [DataField]
    public bool RemoveExisting = true;

    public override void AfterEquip(EntityUid mob)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        entMan.AddComponents(mob, Components, removeExisting: RemoveExisting);
    }
}
