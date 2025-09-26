// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 MarkerWicker <markerWicker@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Body.Systems;
using Content.Shared.CCVar;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared._EinsteinEngines.Contests;
using Content.Shared._EinsteinEngines.HeightAdjust;
using Content.Shared.Body.Components;
using Robust.Shared.Configuration;

namespace Content.Server._EinsteinEngines.HeightAdjust;

public sealed class BloodstreamAdjustSystem : EntitySystem
{
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly ContestsSystem _contests = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BloodstreamAffectedByMassComponent, MapInitEvent>((uid, comp, _) => TryAdjustBloodstream((uid, comp)));
        SubscribeLocalEvent<BloodstreamAffectedByMassComponent, HeightAdjustedEvent>((uid, comp, _) => TryAdjustBloodstream((uid, comp)));
    }

    /// <summary>
    ///     Adjusts the bloodstream of the specified entity based on the settings provided by the component.
    /// </summary>
    public bool TryAdjustBloodstream(Entity<BloodstreamAffectedByMassComponent> ent)
    {
        if (!TryComp<BloodstreamComponent>(ent, out var bloodstream)
            || !_solutionContainer.TryGetSolution(ent.Owner, bloodstream.BloodSolutionName, out var bloodSolutionEnt)
            || bloodstream.BloodMaxVolume == 0
            || !_config.GetCVar(CCVars.HeightAdjustModifiesBloodstream))
            return false;

        var bloodSolution = bloodSolutionEnt.Value.Comp.Solution;

        var factor = Math.Pow(_contests.MassContest(ent, bypassClamp: true, rangeFactor: 4f), ent.Comp.Power);
        factor = Math.Clamp(factor, ent.Comp.Min, ent.Comp.Max);

        var newVolume = bloodstream.BloodMaxVolume * factor;
        var newBloodLevel = bloodSolution.FillFraction * newVolume;
        bloodSolution.MaxVolume = newVolume;
        bloodSolution.SetContents([new ReagentQuantity(bloodstream.BloodReagent, newBloodLevel, null)], false);

        _bloodstream.SetBloodMaxVolume((ent.Owner, bloodstream), newVolume);

        return true;
    }
}
