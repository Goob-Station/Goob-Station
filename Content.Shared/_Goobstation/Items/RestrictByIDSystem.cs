using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Shared._Goobstation.Items
{
    public sealed partial class RestrictByIDSystem : EntitySystem
    {
        [Dependency] private readonly AccessReaderSystem _accessReader = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<RestrictByIdComponent, ComponentInit>(OnComponentInit);
            SubscribeLocalEvent<RestrictByIdComponent, AttemptShootEvent>(OnAttemptShoot);
            SubscribeLocalEvent<RestrictByIdComponent, AttemptMeleeEvent>(OnAttemptMelee);
        }

        public void OnComponentInit(Entity<RestrictByIdComponent> ent, ref ComponentInit args)
        {
            var item = ent.Comp.Owner;

            // Try to find the access reader component, if it doesn't exist, return.
            if (!(TryComp<AccessReaderComponent>(item, out var readerAccess)))
                return;

            // Set the access levels.
            _accessReader.SetAccesses(item, readerAccess, [ent.Comp.RestrictTo]);
        }

        private void OnAttemptShoot(Entity<RestrictByIdComponent> ent, ref AttemptShootEvent args)
        {
            var attacker = args.User;
            var item = ent.Comp.Owner;

            // If the entity shooting the item is invalid, return.
            if (!attacker.IsValid() || !item.IsValid() || !ent.Comp.RestrictMelee)
                return;

            // if the entities ID card does not match the allowed accesses, cancel.
            if (!_accessReader.IsAllowed(attacker, item))
                args.Cancelled = true;
        }

        private void OnAttemptMelee(Entity<RestrictByIdComponent> ent, ref AttemptMeleeEvent args)
        {
            var attacker = args.User;
            var item = ent.Comp.Owner;

            // If the entity swinging the weapon is invalid, return.
            if (!attacker.IsValid() || !item.IsValid() || !ent.Comp.RestrictRanged)
                return;

            // if the entities ID card does not match the allowed accesses, cancel.
            if (!_accessReader.IsAllowed(attacker, item))
                args.Cancelled = true;
        }


    }
}
