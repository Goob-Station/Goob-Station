using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;

namespace Content.Goobstation.Shared.MartialArts;

public sealed partial class MartialArtsComboSystem
{
    [SubscribeLocalEvent]
    private void OnUserInteractHandEvent(Entity<MobStateComponent> ent, ref UserInteractHandEvent args)
    {
        RaiseLocalEvent(args.User, new ComboAttackPerformedEvent(args.User, args.Target, args.User, ComboAttackType.Hug));
    }
}
