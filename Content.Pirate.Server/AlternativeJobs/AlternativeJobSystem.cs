using Content.Pirate.Common.AlternativeJobs;
using Content.Server.Access.Systems;
using Content.Shared.Access.Components;
using Robust.Shared.Prototypes;

namespace Content.Pirate.Server.AlternativeJobs;

public sealed class AlternativeJobSystem : IAlternativeJob
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
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
}
