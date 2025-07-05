using Content.Pirate.Common.AlternativeJobs;
using Content.Server.Access.Systems;
using Content.Server.GameTicking;
using Content.Server.Mind;
using Content.Server.Station.Systems;
using Content.Shared.Access.Components;
using Content.Shared.Mind;
using Content.Shared.Preferences;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Pirate.Server.AlternativeJobs;

public sealed class AlternativeJobSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IdCardSystem _idCardSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AlternativeJobComponent, PlayerSpawningEvent>(OnPlayerSpawned);
    }

    private void OnPlayerSpawned(EntityUid uid, AlternativeJobComponent component, PlayerSpawningEvent args)
    {
        // If it's not current player spawned, ignore
        if (uid != args.SpawnResult)
            return;

        // If no job is set, ignore
        if (args.Job is null) return;
        if (args.HumanoidCharacterProfile is null) return;
        // Check player for idCard
        if (!_idCardSystem.TryFindIdCard(uid, out var idCard)) return;
        // Check card for component just in case
        if (!TryComp<IdCardComponent>(idCard, out var idCardComp)) return;
        // Get alternative job proto based on player preferences and job
        if (!TryGetAlternativeJob(args.Job, args.HumanoidCharacterProfile, out var alternativeJobPrototype)) return;
        // Change job title on id card
        _idCardSystem.TryChangeJobTitle(idCard, alternativeJobPrototype.LocalizedJobName, idCardComp);
    }

    public bool TryGetAlternativeJob(string parentJobId, HumanoidCharacterProfile profile, out AlternativeJobPrototype alternativeJobPrototype)
    {
        if (profile.JobAlternatives.TryGetValue(parentJobId, out var alternativeJobId))
        {
            if (_prototypeManager.TryIndex<AlternativeJobPrototype>(alternativeJobId, out var altJobPrototype))
            {
                alternativeJobPrototype = altJobPrototype;
                return true;
            }
        }

        alternativeJobPrototype = default!;
        return false;
    }
}
