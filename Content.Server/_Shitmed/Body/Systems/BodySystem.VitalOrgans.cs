using Content.Server.Body.Components;
using Content.Shared._Shitmed.Body.Organ;
using Content.Shared.Body.Components;

namespace Content.Server.Body.Systems;

public partial class BodySystem
{
    /// <summary>
    /// Returns whether an entity is missing a brain and heart.
    /// If it does not have a body this returns false.
    /// </summary>
    public bool MissingVitalOrgans(EntityUid uid)
    {
        if (!TryComp<BodyComponent>(uid, out var body))
            return false; // no organs to be missing

        var ent = (uid, body);
        if (!TryGetBodyOrganEntityComps<BrainComponent>(ent, out _))
            return false;

        return TryGetBodyOrganEntityComps<HeartComponent>(ent, out _);
    }
}
