// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.SlaughterDemon.Items;
using Content.Goobstation.Shared.SlaughterDemon.Objectives;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.SlaughterDemon.Objectives;

/// <summary>
/// A lot of the objectives are fluff. The actual ones are listed in this system.
/// </summary>
public sealed class SlaughterDemonObjectiveSystem : EntitySystem
{
    [Dependency] private readonly NumberObjectiveSystem _number = default!;
    [Dependency] private readonly TargetObjectiveSystem _target = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        // Sets progress
        SubscribeLocalEvent<SlaughterDevourConditionComponent, ObjectiveGetProgressEvent>(OnGetDevourProgress);
        SubscribeLocalEvent<SlaughterKillEveryoneConditionComponent, ObjectiveGetProgressEvent>(OnGetKillEveryoneProgress);
        SubscribeLocalEvent<SlaughterKillTheWizardConditionComponent, ObjectiveGetProgressEvent>(OnGetWizardKillProgress);
        SubscribeLocalEvent<SlaughterBaseObjectiveComponent, ObjectiveGetProgressEvent>(OnGetBaseObjectiveProgress);

        // Sets the wizard
        SubscribeLocalEvent<SlaughterKillTheWizardConditionComponent, ObjectiveAssignedEvent>(OnAssignWizard);
        // Fluff objectives
        SubscribeLocalEvent<SlaughterSpreadBloodObjectiveComponent, ObjectiveAssignedEvent>(OnSpreadAssign);

        // Sets descriptions and titles
        SubscribeLocalEvent<SlaughterBaseObjectiveComponent, ObjectiveAfterAssignEvent>(OnAfterAssignObjective);
        SubscribeLocalEvent<SlaughterKillTheWizardConditionComponent, ObjectiveAfterAssignEvent>(OnAfterKillTheWizardAssignObjective);
        SubscribeLocalEvent<SlaughterKillEveryoneConditionComponent, ObjectiveAfterAssignEvent>(OnAfterKillEveryoneAssignObjective);

    }

    private void OnAfterKillTheWizardAssignObjective(Entity<SlaughterKillTheWizardConditionComponent> ent,
        ref ObjectiveAfterAssignEvent args)
    {
        // rouden = goiden 💔💔💨
        if (ent.Comp.Title != null)
            _meta.SetEntityName(ent.Owner, Loc.GetString(ent.Comp.Title), args.Meta);

        if (ent.Comp.Description != null)
            _meta.SetEntityDescription(ent.Owner, Loc.GetString(ent.Comp.Description), args.Meta);
    }

    private void OnAfterKillEveryoneAssignObjective(Entity<SlaughterKillEveryoneConditionComponent> ent,
        ref ObjectiveAfterAssignEvent args)
    {
        if (ent.Comp.Title != null)
            _meta.SetEntityName(ent.Owner, Loc.GetString(ent.Comp.Title), args.Meta);

        if (ent.Comp.Description != null)
            _meta.SetEntityDescription(ent.Owner, Loc.GetString(ent.Comp.Description), args.Meta);
    }

    private void OnAfterAssignObjective(Entity<SlaughterBaseObjectiveComponent> ent,
        ref ObjectiveAfterAssignEvent args)
    {
        if (ent.Comp.Title != null)
            _meta.SetEntityName(ent.Owner, Loc.GetString(ent.Comp.Title), args.Meta);

        if (ent.Comp.Description != null)
            _meta.SetEntityDescription(ent.Owner, Loc.GetString(ent.Comp.Description), args.Meta);
    }

    private void OnSpreadAssign(Entity<SlaughterSpreadBloodObjectiveComponent> ent, ref ObjectiveAssignedEvent args)
    {
        if (ent.Comp.Title == null)
            return;

        var areas = ent.Comp.Areas;
        var randomArea = _random.Pick(areas);
        var title = Loc.GetString(ent.Comp.Title, ("area", randomArea));

        _meta.SetEntityName(ent.Owner, title);
    }

    private void OnAssignWizard(Entity<SlaughterKillTheWizardConditionComponent> ent, ref ObjectiveAssignedEvent args)
    {
        if (!TryComp<TargetObjectiveComponent>(ent.Owner, out var targetObjective))
            return;

        var query = EntityQueryEnumerator<VialSummonComponent>();
        while (query.MoveNext(out _, out var comp))
        {
            if (comp.Used || comp.Summoner == null)
                continue;

            _target.SetTarget(ent.Owner, comp.Summoner.Value, targetObjective);
            comp.Used = true;
            return;
        }
    }

    private void OnGetBaseObjectiveProgress(Entity<SlaughterBaseObjectiveComponent> ent, ref ObjectiveGetProgressEvent args) =>
        args.Progress = 0.0f;

    private void OnGetKillEveryoneProgress(Entity<SlaughterKillEveryoneConditionComponent> ent, ref ObjectiveGetProgressEvent args) =>
        args.Progress = Progress(ent.Comp.Devoured, GetAllPlayers());

    private void OnGetDevourProgress(Entity<SlaughterDevourConditionComponent> ent, ref ObjectiveGetProgressEvent args) =>
        args.Progress = Progress(ent.Comp.Devour, _number.GetTarget(ent.Owner));

    private void OnGetWizardKillProgress(Entity<SlaughterKillTheWizardConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        if (!_target.GetTarget(ent.Owner, out var targetUid))
        {
            args.Progress = 0f;
            return;
        }

        args.Progress = GetWizardKillProgress(targetUid.Value);
    }

    private float GetWizardKillProgress(EntityUid target)
    {
        if (!_mind.TryGetMind(target, out _, out var mind))
            return 1f;

        return !_mind.IsCharacterDeadIc(mind) ? 0f : 1f;
    }

    private static float Progress(int recruited, int target)
    {
        // prevent divide-by-zero
        return target == 0 ? 1f : MathF.Min(recruited / (float) target, 1f);
    }

    private int GetAllPlayers()
    {
        return EntityQuery<HumanoidAppearanceComponent, ActorComponent>().Count();
    }
}
