using Content.Shared.Item.ItemToggle;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Shared.Augments;

public sealed partial class AugmentStrengthSystem : EntitySystem
{
    [Dependency] private ItemToggleSystem _toggle = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AugmentStrengthComponent, GetUserMeleeDamageEvent>(OnGetMeleeDamage);
    }

    private void OnGetMeleeDamage(Entity<AugmentStrengthComponent> ent, ref GetUserMeleeDamageEvent args)
    {
        if (_toggle.IsActivated(ent.Owner))
            args.Damage *= ent.Comp.Modifier;
    }
}
