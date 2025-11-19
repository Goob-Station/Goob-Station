using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Shared.Augments;

public sealed class AugmentRelaySystem : EntitySystem
{
    [Dependency] private readonly AugmentSystem _augment = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InstalledAugmentsComponent, GetUserMeleeDamageEvent>(_augment.RelayEvent);
    }
}
