using Content.Goobstation.Shared.MartialArts.Events;
using Content.Goobstation.Shared.Medical.Components;

namespace Content.Goobstation.Shared.Medical.Systems;

public sealed partial class LobotomizedSystem : EntitySystem
{
    [SubscribeLocalEvent]
    private void AfterMovePerformed(Entity<LobotomizedComponent> ent, ref AfterComboAttackPerformedEvent args)
    {
        args.Cancel(); // funny
    }
}
