using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Systems;

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
            var comp = ent.Comp;

            args.Message = Loc.GetString(comp.FailText);

            // If the entity shooting the item is invalid, return.
            if (!attacker.IsValid() || !item.IsValid() || !ent.Comp.RestrictMelee)
                return;

            // If the entities ID card matches the allowed accesses, and invert is false, return and allow the shot.
            if (_accessReader.IsAllowed(attacker, item) && !comp.Invert)
                return;
            // If the entities ID card doesn't match the allowed accesses, but invert is true, return and allow the shot.
            if (!_accessReader.IsAllowed(attacker, item) && comp.Invert)
                return;

            args.Cancelled = true;
            _popupSystem.PopupClient(args.Message, item, PopupType.MediumCaution);

        }

        private void OnAttemptMelee(Entity<RestrictByIdComponent> ent, ref AttemptMeleeEvent args)
        {
            var attacker = args.User;
            var item = ent.Owner;
            var comp = ent.Comp;

            args.Message = Loc.GetString(comp.FailText);

            // If the entity swinging the weapon is invalid, return.
            if (!attacker.IsValid() || !item.IsValid() || !comp.RestrictRanged)
                return;

            // If the entities ID card matches the allowed accesses, and invert is false, return and allow the shot.
            if (_accessReader.IsAllowed(attacker, item) && !comp.Invert)
                return;
            // If the entities ID card doesn't match the allowed accesses, but invert is true, return and allow the shot.
            if (!_accessReader.IsAllowed(attacker, item) && comp.Invert)
                return;

            args.Cancelled = true;
            _popupSystem.PopupClient(args.Message, item, PopupType.MediumCaution);
        }
    }
}
