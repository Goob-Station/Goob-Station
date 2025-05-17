using Content.Goobstation.Shared.Gatherable;
using Content.Server.Gatherable;

namespace Content.Goobstation.Server.Gatherable;

public sealed class ToolGatherableSystem : SharedToolGatherableSystem
{
    [Dependency] private readonly GatherableSystem _gatherable = default!;

    protected override void Gather(Entity<ToolGatherableComponent> ent, EntityUid user)
    {
        base.Gather(ent, user);

        _gatherable.Gather(ent.Owner, user);
    }
}
