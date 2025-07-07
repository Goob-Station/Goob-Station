using Content.Pirate.Common.AlternativeJobs;
using Content.Server.Access.Components;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Preferences;
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
        // If no job is set, ignore
        if (args.JobId is null) return;
        if (args.Profile is null) return;
        // Check player for idCard
        if (!_idCardSystem.TryFindIdCard(uid, out var idCard)) return;
        // Check card for component just in case
        if (!TryComp<IdCardComponent>(idCard, out var idCardComp)) return;
        if (!idCardComp.Initialized)
            return;

        // Get alternative job proto based on player preferences and job
        if (!TryGetAlternativeJob(args.JobId, args.Profile, out var alternativeJobPrototype)) return;
        if (TryComp<PresetIdCardComponent>(idCard, out var presetIdCardComp)) { presetIdCardComp.JobName = alternativeJobPrototype.LocalizedJobName; }
        _idCardSystem.TryChangeJobTitle(idCard, alternativeJobPrototype.LocalizedJobName, idCardComp);
        idCardComp.LocalizedJobTitle = alternativeJobPrototype.LocalizedJobName;
        // Change job title on id card

        Dirty(idCard, idCardComp);
    }

    public bool TryGetAlternativeJob(string parentJobId, HumanoidCharacterProfile profile, out AlternativeJobPrototype alternativeJobPrototype)
    {
        if (profile.JobAlternatives.TryGetValue(parentJobId, out var alternativeJobId))
        {
            if (_prototypeManager.TryIndex(alternativeJobId, out var altJobPrototype))
            {
                alternativeJobPrototype = altJobPrototype;
                return true;
            }
        }

        alternativeJobPrototype = default!;
        return false;
    }
}
