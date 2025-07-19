using Content.Shared.IdentityManagement;
using Content.Shared.IdentityManagement.Components;
using Content.Shared.Psionics;
using Content.Shared.Psionics.Components;

/// <summary>
/// EVERYTHING HERE IS A MODIFIED VERSION OF CRIMINAL RECORDS
/// </summary>

namespace Content.Shared.PsionicsRecords.Systems;

public abstract class SharedPsionicsRecordsConsoleSystem : EntitySystem
{
    /// <summary>
    /// Any entity that has the name of the record that was just changed as their visible name will get their icon
    /// updated with the new status, if the record got removed their icon will be removed too.
    /// </summary>
    public void UpdatePsionicsIdentity(string name, PsionicsStatus status)
    {
        var query = EntityQueryEnumerator<IdentityComponent>();

        while (query.MoveNext(out var uid, out var identity))
        {
            if (!Identity.Name(uid, EntityManager).Equals(name))
                continue;

            if (status == PsionicsStatus.None)
                RemComp<PsionicsRecordComponent>(uid);
            else
                SetPsionicsIcon(name, status, uid);
        }
    }

    /// <summary>
    /// Decides the icon that should be displayed on the entity based on the psionics status
    /// </summary>
    public void SetPsionicsIcon(string name, PsionicsStatus status, EntityUid characterUid)
    {
        EnsureComp<PsionicsRecordComponent>(characterUid, out var record);

        var previousIcon = record.StatusIcon;

        record.StatusIcon = status switch
        {
            PsionicsStatus.Suspected => "PsionicsIconSuspected",
            PsionicsStatus.Registered => "PsionicsIconRegistered",
            PsionicsStatus.Abusing => "PsionicsIconAbusing",
            _ => record.StatusIcon
        };

        if (previousIcon != record.StatusIcon)
            Dirty(characterUid, record);
    }
}
