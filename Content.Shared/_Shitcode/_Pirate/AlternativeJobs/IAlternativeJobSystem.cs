using Content.Pirate.Common.AlternativeJobs;
using Content.Shared.Preferences;

namespace Content.Shared._Shitcode._Pirate.AlternativeJobs
{
    public interface IAlternativeJobSystem
    {
        bool TryGetAlternativeJob(string parentJobId, HumanoidCharacterProfile profile, out AlternativeJobPrototype alternativeJobPrototype);
    }
}
