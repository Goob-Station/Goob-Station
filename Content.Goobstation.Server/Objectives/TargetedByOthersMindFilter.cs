using Content.Server.Objectives.Components;
using Content.Shared.Mind;
using Content.Shared.Mind.Filters;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Server.Objectives;

public sealed partial class TargetedByOthersMindFilter : MindFilter
{
    [DataField(required: true)]
    public EntityWhitelist Whitelist = new();

    protected override bool ShouldRemove(Entity<MindComponent> mind, EntityUid? exclude, IEntityManager entMan, SharedMindSystem mindSys)
    {
        var whitelistSys = entMan.System<EntityWhitelistSystem>();
        var query = entMan.EntityQueryEnumerator<MindComponent>();
        while (query.MoveNext(out var otherUid, out var other))
        {
            if (otherUid == exclude || otherUid == mind.Owner)
                continue;

            foreach (var objective in other.Objectives)
            {
                if (entMan.TryGetComponent<TargetObjectiveComponent>(objective, out var target)
                    && target.Target == mind.Owner
                    && whitelistSys.IsWhitelistPass(Whitelist, objective))
                    return false;
            }
        }

        return true;
    }
}
