using System.Linq;
using Content.Shared.Access;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Items
{
    public sealed partial class RestrictByIdSystem : EntitySystem
    {
        [Dependency] private readonly AccessReaderSystem _accessReader = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<RestrictByIdComponent, ComponentInit>(OnComponentInit);
            SubscribeLocalEvent<RestrictByIdComponent, AttemptShootEvent>(OnAttemptShoot);
            SubscribeLocalEvent<RestrictByIdComponent, AttemptMeleeEvent>(OnAttemptMelee);
        }

        private void OnComponentInit(Entity<RestrictByIdComponent> ent, ref ComponentInit args)
        {
            var item = ent.Owner;

            // Set the access levels.
            EnsureComp<AccessReaderComponent>(item, out var accessReader);
            foreach (var access in ent.Comp.AccessLists){}
            accessReader.AccessLists.Add(ent.Comp.AccessLists);
        }

        private void OnAttemptShoot(Entity<RestrictByIdComponent> ent, ref AttemptShootEvent args)
        {
            var attacker = args.User;
            var item = ent.Owner;

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
            var item = ent.Owner;

            // If the entity swinging the weapon is invalid, return.
            if (!attacker.IsValid() || !item.IsValid() || !ent.Comp.RestrictRanged)
                return;

            // if the entities ID card does not match the allowed accesses, cancel.
            if (!_accessReader.IsAllowed(attacker, item))
                args.Cancelled = true;
        }


    }
}
