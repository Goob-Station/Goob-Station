using System.Diagnostics.CodeAnalysis;
using Content.Shared.StatusIcon; // GabyStation radio icons

namespace Content.Shared.Roles.Jobs;

public abstract partial class SharedJobSystem
{

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

