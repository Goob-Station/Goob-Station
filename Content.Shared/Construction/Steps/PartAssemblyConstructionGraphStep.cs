// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Construction.Components;
using Content.Shared.Examine;

namespace Content.Shared.Construction.Steps;

[DataDefinition]
public sealed partial class PartAssemblyConstructionGraphStep : ConstructionGraphStep
{
    /// <summary>
    /// A valid ID on <see cref="PartAssemblyComponent"/>'s dictionary of strings to part lists.
    /// </summary>
    [DataField]
    public string AssemblyId = string.Empty;

    /// <summary>
    /// A localization string used when examining and for the guidebook.
    /// </summary>
    [DataField]
    public LocId GuideString = "construction-guide-condition-part-assembly";

    public bool Condition(EntityUid uid, IEntityManager entityManager)
    {
        return entityManager.System<PartAssemblySystem>().IsAssemblyFinished(uid, AssemblyId);
    }

    public override void DoExamine(ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString(GuideString));
    }

    public override ConstructionGuideEntry GenerateGuideEntry()
    {
        return new ConstructionGuideEntry
        {
            Localization = GuideString,
        };
    }
}