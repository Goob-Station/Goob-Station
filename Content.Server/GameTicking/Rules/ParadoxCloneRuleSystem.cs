// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Antag;
using Content.Server.Antag.Components;
using Content.Server.Cloning;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Medical.SuitSensors;
using Content.Server.Objectives.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Gibbing.Components;
using Content.Shared.Medical.SuitSensor;
using Content.Shared.Mind;
using Content.Shared.Whitelist;
using Content.Server.Objectives.Systems; // Pirate edit
using Content.Shared.Objectives.Systems; // Pirate edit
using Content.Shared.Objectives.Components; // Pirate edit
using Robust.Shared.Random;

namespace Content.Server.GameTicking.Rules;

public sealed class ParadoxCloneRuleSystem : GameRuleSystem<ParadoxCloneRuleComponent>
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly CloningSystem _cloning = default!;
    [Dependency] private readonly SuitSensorSystem _sensor = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!; // Goobstation
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!; // Pirate edit
    [Dependency] private readonly TargetObjectiveSystem _targetObjective = default!; // Pirate edit

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ParadoxCloneRuleComponent, AntagSelectEntityEvent>(OnAntagSelectEntity);
        SubscribeLocalEvent<ParadoxCloneRuleComponent, AfterAntagEntitySelectedEvent>(AfterAntagEntitySelected);
    }

    protected override void Started(EntityUid uid, ParadoxCloneRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        // check if we got enough potential cloning targets, otherwise cancel the gamerule so that the ghost role does not show up
        var allHumans = _mind.GetAliveHumans();
        allHumans.RemoveWhere(human => _whitelist.IsWhitelistPass(component.TargetBlacklist, human)); // Goobstation

        if (allHumans.Count == 0)
        {
            Log.Info("Could not find any alive players to create a paradox clone from! Ending gamerule.");
            ForceEndSelf(uid, gameRule);
        }
    }


    // we have to do the spawning here so we can transfer the mind to the correct entity and can assign the objectives correctly
    private void OnAntagSelectEntity(Entity<ParadoxCloneRuleComponent> ent, ref AntagSelectEntityEvent args)
    {
        if (args.Session?.AttachedEntity is not { } spawner)
            return;

        if (ent.Comp.OriginalBody != null) // target was overridden, for example by admin antag control
        {
            if (Deleted(ent.Comp.OriginalBody.Value) || !_mind.TryGetMind(ent.Comp.OriginalBody.Value, out var originalMindId, out var _))
            {
                Log.Warning("Could not find mind of target player to paradox clone!");
                return;
            }
            ent.Comp.OriginalMind = originalMindId;
        }
        else
        {
            // get possible targets
            var allAliveHumanoids = _mind.GetAliveHumans();
            allAliveHumanoids.RemoveWhere(human => _whitelist.IsWhitelistPass(ent.Comp.TargetBlacklist, human)); // Goobstation

            // we already checked when starting the gamerule, but someone might have died since then.
            if (allAliveHumanoids.Count == 0)
            {
                Log.Warning("Could not find any alive players to create a paradox clone from!");
                return;
            }

            // pick a random player
            var randomHumanoidMind = _random.Pick(allAliveHumanoids);
            ent.Comp.OriginalMind = randomHumanoidMind;
            ent.Comp.OriginalBody = randomHumanoidMind.Comp.OwnedEntity;

        }

        if (ent.Comp.OriginalBody == null || !_cloning.TryCloning(ent.Comp.OriginalBody.Value, _transform.GetMapCoordinates(spawner), ent.Comp.Settings, out var clone))
        {
            Log.Error($"Unable to make a paradox clone of entity {ToPrettyString(ent.Comp.OriginalBody)}");
            return;
        }

        // Pirate edit start VVV
        var targetOverride = EnsureComp<TargetOverrideComponent>(clone.Value);
        targetOverride.Target = ent.Comp.OriginalMind;

        var syncComp = EnsureComp<ParadoxSyncComponent>(clone.Value);
        syncComp.Target = ent.Comp.OriginalBody.Value;
        syncComp.EffectProto = ent.Comp.GibProto;
        // Pirate edit end ^^^

        // turn their suit sensors off so they don't immediately get noticed
        _sensor.SetAllSensors(clone.Value, SuitSensorMode.SensorOff);

        args.Entity = clone;
    }

    private void AfterAntagEntitySelected(Entity<ParadoxCloneRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        if (ent.Comp.OriginalMind == null)
            return;

        if (!_mind.TryGetMind(args.EntityUid, out var cloneMindId, out var cloneMindComp))
            return;

        _mind.CopyObjectives(ent.Comp.OriginalMind.Value, (cloneMindId, cloneMindComp), ent.Comp.ObjectiveWhitelist, ent.Comp.ObjectiveBlacklist);

        // Pirate edit start VVV
        if (_objectives.TryCreateObjective((cloneMindId, cloneMindComp), "ParadoxCloneFriendObjective", out var objective))
        {
            _targetObjective.SetTarget(objective.Value, ent.Comp.OriginalMind.Value);
            
            _mind.AddObjective(cloneMindId, cloneMindComp, objective.Value);
        }
        // Pirate edit end ^^^
    }
}