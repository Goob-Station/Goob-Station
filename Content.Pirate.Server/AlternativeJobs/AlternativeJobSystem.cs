using Content.Pirate.Common.AlternativeJobs;
using Content.Server.Access.Systems;
using Content.Shared.Access.Components;
using Robust.Shared.Prototypes;

namespace Content.Pirate.Server.AlternativeJobs;

public sealed class AlternativeJobSystem : EntitySystem, IAlternativeJob
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IdCardSystem _idCardSystem = default!;

    public bool TryGetAlternativeJob(string parentJobId, out AlternativeJobPrototype alternativeJobPrototype)
    {
        foreach (var prototype in _prototypeManager.EnumeratePrototypes<AlternativeJobPrototype>())
        {
            if (prototype.ParentJobId == parentJobId)
            {
                alternativeJobPrototype = prototype;
                return true;
            }
        }
        alternativeJobPrototype = default!;
        return false;
    }
    public bool TrySetJobTitle(EntityUid cardUid, string newJobTitie)
    {
        if (!TryComp<IdCardComponent>(cardUid, out var idCardComp)) return false;
        return _idCardSystem.TryChangeJobTitle(cardUid, newJobTitie, idCardComp);
    }
}
