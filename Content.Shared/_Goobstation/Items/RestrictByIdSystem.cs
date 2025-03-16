using System.Linq;
using Content.Shared.Access;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Items
{
    public sealed partial class RestrictByIdSystem : EntitySystem
    {
        [Dependency] private readonly AccessReaderSystem _accessReader = default!;
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<RestrictByIdComponent, ComponentInit>(OnComponentInit);
            SubscribeLocalEvent<RestrictByIdComponent, AttemptShootEvent>(OnAttemptShoot);
            SubscribeLocalEvent<RestrictByIdComponent, AttemptMeleeEvent>(OnAttemptMelee);
        }

        private void OnComponentInit(Entity<RestrictByIdComponent> ent, ref ComponentInit args)
        {
            var item = ent.Comp.Owner;

            // Set the access levels.
            EnsureComp<AccessReaderComponent>(item, out var accessReader);
            _accessReader.SetAccesses(item, accessReader, ent.Comp.AccessLists);
        }

        private void OnAttemptShoot(Entity<RestrictByIdComponent> ent, ref AttemptShootEvent args)
        {
            var attacker = args.User;
            var item = ent.Owner;

            // If the entity shooting the item is invalid, return.
            if (!attacker.IsValid() || !item.IsValid() || !ent.Comp.RestrictMelee)
                return;

            // If the entities ID card does not match the allowed accesses, cancel.
            if (!_accessReader.IsAllowed(attacker, item) && !ent.Comp.Invert)
            {
                args.Cancelled = true;
            }

            // If the entities ID card *does* match the allowed accesses, but invert is on, cancel.
            else if (_accessReader.IsAllowed(attacker, item) && ent.Comp.Invert)
                args.Cancelled = true;
        }

        private void OnAttemptMelee(Entity<RestrictByIdComponent> ent, ref AttemptMeleeEvent args)
        {
            var attacker = args.User;
            var item = ent.Owner;

            // If the entity swinging the weapon is invalid, return.
            if (!attacker.IsValid() || !item.IsValid() || !ent.Comp.RestrictRanged)
                return;

            // If the entities ID card does not match the allowed accesses, cancel.
            if (!_accessReader.IsAllowed(attacker, item) && !ent.Comp.Invert)
                args.Cancelled = true;
            // If the entities ID card *does* match the allowed accesses, but invert is on, cancel.
            else if (_accessReader.IsAllowed(attacker, item) && ent.Comp.Invert)
                args.Cancelled = true;
        }


    }
}
