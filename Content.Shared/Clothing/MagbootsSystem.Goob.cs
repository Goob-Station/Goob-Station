using Content.Goobstation.Common.Gravity;
using Content.Shared.Item.ItemToggle.Components;

namespace Content.Shared.Clothing;

public sealed partial class SharedMagbootsSystem
{
    private void OnToggleActivateAttempt(Entity<MagbootsComponent> ent, ref ItemToggleActivateAttemptEvent args)
    {
        if (TryComp<FlipGravityComponent>(args.User, out var flip))
        {
            _electro.TryDoElectrocution(args.User.Value, null, flip.Damage, flip.Duration, true, ignoreInsulation: true);
            args.Cancelled = true;
        }
    }
}
