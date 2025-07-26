using Content.Goobstation.Server.Silicon.MalfAI.Objectives;
using Content.Goobstation.Shared.Silicon.MalfAI;
using Content.Server.Objectives.Systems;
using Content.Server.Shuttles.Systems;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.Humanoid;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Silicon.MalfAI;

public sealed partial class MalfStationAISystem : SharedMalfStationAISystem
{
    [Dependency] private readonly EmergencyShuttleSystem _emergencyShuttle = default!;
    [Dependency] private readonly TargetObjectiveSystem _targeting = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly IRobustRandom _robustRandom = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private void InitializeObjectives()
    {
        SubscribeLocalEvent<NoOrganicEscapeObjectiveComponent, ObjectiveGetProgressEvent>(OnNoOrganicEscapeGetProgress);
        SubscribeLocalEvent<YandereObjectiveComponent, ObjectiveGetProgressEvent>(OnYandereGetProgress);

        SubscribeLocalEvent<PurgeSpeciesObjectiveComponent, ObjectiveAssignedEvent>(OnPurgeAssigned);
        SubscribeLocalEvent<PurgeSpeciesObjectiveComponent, ObjectiveGetProgressEvent>(OnPurgeGetProgress);
    }

    private void OnPurgeAssigned(Entity<PurgeSpeciesObjectiveComponent> ent, ref ObjectiveAssignedEvent args)
    {
        var i = _robustRandom.Next(ent.Comp.SpeciesWhitelist.Count - 1);

        ent.Comp.TargetSpeciesPrototype = ent.Comp.SpeciesWhitelist[i];

        var speciesName = Loc.GetString(_prototype.Index(ent.Comp.TargetSpeciesPrototype).Name);

        _meta.SetEntityName(ent, Loc.GetString(ent.Comp.TitleLoc, ("species", speciesName)));
    }

    private void OnPurgeGetProgress(Entity<PurgeSpeciesObjectiveComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        // Mmm, space racism.

        args.Progress = 0.0f;

        if (!_emergencyShuttle.EmergencyShuttleArrived)
        {
            args.Progress = 1.0f;
            return;
        }

        var query = EntityQuery<MindComponent>();

        foreach (var mind in query)
        {
            if (mind.OwnedEntity == null)
                continue;

            if (HasComp<SiliconComponent>(mind.OwnedEntity.Value))
                continue;

            if (!TryComp<HumanoidAppearanceComponent>(mind.OwnedEntity, out var appearance) || appearance.Species != ent.Comp.TargetSpeciesPrototype)
                continue;

            if (_emergencyShuttle.IsTargetEscaping(mind.OwnedEntity.Value))
                return;
        }

        args.Progress = 1.0f;
    }

    private void OnYandereGetProgress(Entity<YandereObjectiveComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = 0f;

        if (!_targeting.GetTarget(ent, out var target))
            return;

        if (!TryComp<MindComponent>(target, out var mind) || mind.OwnedEntity == null)
            return;

        if (_mind.IsCharacterDeadIc(mind))
            return;

        if (_emergencyShuttle.EmergencyShuttleArrived && _emergencyShuttle.IsTargetEscaping(mind.OwnedEntity.Value))
            return;

        args.Progress = 1f;
    }

    private void OnNoOrganicEscapeGetProgress(Entity<NoOrganicEscapeObjectiveComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = 0.0f;

        if (!_emergencyShuttle.EmergencyShuttleArrived)
        {
            args.Progress = 1.0f;
            return;
        }

        var query = EntityQuery<MindComponent>();

        foreach (var mind in query)
        {
            if (mind.OwnedEntity == null)
                continue;

            if (HasComp<SiliconComponent>(mind.OwnedEntity.Value))
                continue;

            if (_emergencyShuttle.IsTargetEscaping(mind.OwnedEntity.Value))
                return;
        }

        args.Progress = 1.0f;
    }
}