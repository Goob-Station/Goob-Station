// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Objectives.Components;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Shared.Roles.Jobs;
using System.Diagnostics.CodeAnalysis;
using Content.Server._Goobstation.Wizard.Components;
using Content.Server.Mind;
using Content.Shared.Mind.Components;

namespace Content.Server.Objectives.Systems;

/// <summary>
/// Provides API for other components and handles setting the title.
/// </summary>
public sealed class TargetObjectiveSystem : EntitySystem
{
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedJobSystem _job = default!;
    [Dependency] private readonly MindSystem _mind = default!; // Goobstation

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TargetObjectiveComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);

        SubscribeLocalEvent<DynamicObjectiveTargetMindComponent, MindGotAddedEvent>(OnMindAdded); // Goobstation
        SubscribeLocalEvent<EntityRenamedEvent>(OnRenamed); // Goobstation
    }

    // Goobstation start
    private void OnMindAdded(Entity<DynamicObjectiveTargetMindComponent> ent, ref MindGotAddedEvent args)
    {
        UpdateAllDynamicObjectiveNamesWithTarget(ent.Owner);
    }

    private void OnRenamed(ref EntityRenamedEvent ev)
    {
        if (_mind.TryGetMind(ev.Uid, out var mind, out _) && HasComp<DynamicObjectiveTargetMindComponent>(mind))
            UpdateAllDynamicObjectiveNamesWithTarget(mind);
    }

    private void UpdateAllDynamicObjectiveNamesWithTarget(EntityUid target)
    {
        var query = AllEntityQuery<TargetObjectiveComponent, MetaDataComponent>();

        while (query.MoveNext(out var uid, out var comp, out var meta))
        {
            if (!comp.DynamicName || comp.Target != target)
                continue;

            _metaData.SetEntityName(uid, GetTitle(target, comp.Title, true, comp.ShowJobTitle), meta);
        }
    }

    public void SetName(EntityUid uid, TargetObjectiveComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        if (!GetTarget(uid, out var target, comp))
            return;

        _metaData.SetEntityName(uid, GetTitle(target.Value, comp.Title, comp.DynamicName, comp.ShowJobTitle));
    }
    // Goobstation end

    private void OnAfterAssign(EntityUid uid, TargetObjectiveComponent comp, ref ObjectiveAfterAssignEvent args)
    {
        if (!GetTarget(uid, out var target, comp))
            return;

        _metaData.SetEntityName(uid, GetTitle(target.Value, comp.Title, comp.DynamicName, comp.ShowJobTitle), args.Meta); // Goob edit
    }

    /// <summary>
    /// Sets the Target field for the title and other components to use.
    /// </summary>
    public void SetTarget(EntityUid uid, EntityUid target, TargetObjectiveComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        comp.Target = target;
    }

    /// <summary>
    /// Gets the target from the component.
    /// </summary>
    /// <remarks>
    /// If it is null then the prototype is invalid, just return.
    /// </remarks>
    public bool GetTarget(EntityUid uid, [NotNullWhen(true)] out EntityUid? target, TargetObjectiveComponent? comp = null)
    {
        target = Resolve(uid, ref comp) ? comp.Target : null;
        return target != null;
    }

    private string GetTitle(EntityUid target, string title, bool dynamicName = false, bool showJobTitle = true) // Goob edit
    {
        var targetName = "Unknown";
        // Goob edit start
        if (TryComp<MindComponent>(target, out var mind))
        {
            if (dynamicName && TryComp(mind.OwnedEntity, out MetaDataComponent? meta))
                targetName = meta.EntityName;
            else if (mind.CharacterName != null)
                targetName = mind.CharacterName;
        }

        if (!showJobTitle)
            return Loc.GetString(title, ("targetName", targetName));
        // Goob edit end

        var jobName = _job.MindTryGetJobName(target);
        return Loc.GetString(title, ("targetName", targetName), ("job", jobName));
    }

}