namespace Content.Pirate.Common.AlternativeJobs;

public interface IAlternativeJob
{
    bool TryGetAlternativeJob(string parentJobId, out AlternativeJobPrototype alternativeJobPrototype);
    bool TrySetJobTitle(EntityUid cardUid, string newJobTitie);
}
