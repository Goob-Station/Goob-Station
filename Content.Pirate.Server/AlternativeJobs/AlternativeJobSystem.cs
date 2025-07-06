using Content.Pirate.Common.AlternativeJobs;
using Content.Server.Access.Components;
using Content.Server.Access.Systems;
using Content.Server.GameTicking;
using Content.Server.Mind;
using Content.Server.Station.Systems;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Mind;
using Content.Shared.Preferences;
using Content.Shared.StatusIcon;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Pirate.Server.AlternativeJobs;

public sealed class AlternativeJobSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedIdCardSystem _idCardSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AlternativeJobComponent, PlayerSpawnCompleteEvent>(OnPlayerSpawned);
    }

    private void OnPlayerSpawned(EntityUid uid, AlternativeJobComponent component, PlayerSpawnCompleteEvent args)
    {

        Logger.Info($"PlayerSpawningEvent rised on {uid}");
        // If it's not current player spawned, ignore
        // if (uid != args.SpawnResult)
        //     return;

        // If no job is set, ignore
        Logger.Debug($"Checking for job");
        if (args.JobId is null) return;
        if (args.Profile is null) return;
        // Check player for idCard
        Logger.Debug($"Checking for id card");
        if (!_idCardSystem.TryFindIdCard(uid, out var idCard)) return;
        // Check card for component just in case
        Logger.Debug($"Checking for id card component");
        if (!TryComp<IdCardComponent>(idCard, out var idCardComp)) return;
        if (!idCardComp.Initialized)
        {
            Logger.Error($"Id card not initialized");
            return;
        }
        ;
        // Get alternative job proto based on player preferences and job
        Logger.Debug($"Getting alternative job");
        if (!TryGetAlternativeJob(args.JobId, args.Profile, out var alternativeJobPrototype)) return;
        if (TryComp<PresetIdCardComponent>(idCard, out var presetIdCardComp)) { presetIdCardComp.JobName = alternativeJobPrototype.LocalizedJobName; }
        // Change job title on id card
        Logger.Debug($"Setting job title to {alternativeJobPrototype.LocalizedJobName}");
        Logger.Info($"Result of changing title: {_idCardSystem.TryChangeJobTitle(idCard, alternativeJobPrototype.LocalizedJobName, idCardComp)}");

        Dirty(idCard, idCardComp);
    }

    public bool TryGetAlternativeJob(string parentJobId, HumanoidCharacterProfile profile, out AlternativeJobPrototype alternativeJobPrototype)
    {
        Logger.Debug($"Trying to get alternative job for {parentJobId} for profile of character: {profile.Name}");
        if (profile.JobAlternatives.TryGetValue(parentJobId, out var alternativeJobId))
        {
            Logger.Debug($"Found alternative job id {alternativeJobId} for {parentJobId}. Trying to get prototype...");
            if (_prototypeManager.TryIndex(alternativeJobId, out var altJobPrototype))
            {
                Logger.Debug($"Found alternative job prototype {altJobPrototype.ID} for {parentJobId}");
                alternativeJobPrototype = altJobPrototype;
                return true;
            }
        }

        alternativeJobPrototype = default!;
        return false;
    }
}
