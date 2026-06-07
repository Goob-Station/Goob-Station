using System.Diagnostics.CodeAnalysis;
using Content.Shared.StatusIcon; // GabyStation radio icons
using Robust.Shared.Player;

namespace Content.Shared.Roles.Jobs;

public abstract partial class SharedJobSystem
{

    // Goobstation Change: Returns the amount of Goobcoins a player will receive when they finish a round as this job.
    public int GetJobGoobcoins(ICommonSession player)
    {
        if (_playerSystem.ContentData(player) is not { Mind: { } mindId }
            || !MindTryGetJob(mindId, out var prototype))
            return 1;

        return prototype.Goobcoins;
    }

    // GabyStation
    public bool TryFindJobFromIcon(JobIconPrototype jobIcon, [NotNullWhen(true)] out JobPrototype? job)
    {
        foreach (var jobPrototype in _prototypes.EnumeratePrototypes<JobPrototype>())
        {
            if (jobPrototype.Icon == jobIcon.ID)
            {
                job = jobPrototype;
                return true;
            }
        }

        job = null;
        return false;
    }
    // GabyStation end
}

