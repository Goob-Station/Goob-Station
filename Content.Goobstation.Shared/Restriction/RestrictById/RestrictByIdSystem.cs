// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Emag.Systems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Goobstation.Shared.Restriction.RestrictById
{
    public sealed partial class RestrictByIdSystem : EntitySystem
    {
        [Dependency] private readonly AccessReaderSystem _accessReader = default!;
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
        [Dependency] private readonly EmagSystem _emag = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<RestrictByIdComponent, ComponentInit>(OnComponentInit);
            SubscribeLocalEvent<RestrictByIdComponent, AttemptShootEvent>(OnAttemptShoot);
            SubscribeLocalEvent<RestrictByIdComponent, AttemptMeleeEvent>(OnAttemptMelee);
            SubscribeLocalEvent<RestrictByIdComponent, GotEmaggedEvent>(OnEmagged);
        }

        private void OnComponentInit(Entity<RestrictByIdComponent> ent, ref ComponentInit args)
        {
            var item = ent.Comp.Owner;

            // Set the access levels.
            EnsureComp<AccessReaderComponent>(item, out var accessReader);
            _accessReader.SetAccesses(item, accessReader, ent.Comp.AccessLists);
        }

        private void OnEmagged(Entity<RestrictByIdComponent> ent, ref GotEmaggedEvent args)
        {
            if (!ent.Comp.IsEmaggable)
                return;

            var item = ent.Comp.Owner;

            if (!_emag.CompareFlag(args.Type, EmagType.Interaction))
                return;

            RemComp<AccessReaderComponent>(item);
            ent.Comp.IsEmagged = true;
            args.Handled = true;
            args.Repeatable = false;

        }

        private void OnAttemptShoot(Entity<RestrictByIdComponent> ent, ref AttemptShootEvent args)
        {
            var attacker = args.User;
            var item = ent.Owner;
            var comp = ent.Comp;

            // If the item is currently emagged, do not check for accesses.
            if (comp.IsEmagged)
                return;

            // Get the failtext from the localization string.
            args.Message = Loc.GetString(comp.FailText);

            // If the entity shooting the item is invalid, return.
            if (!attacker.IsValid() || !item.IsValid() || !ent.Comp.RestrictRanged)
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

            // If the item is currently emagged, do not check for accesses.
            if (comp.IsEmagged)
                return;

            // Get the failtext from the localization string.
            args.Message = Loc.GetString(comp.FailText);

            // If the entity swinging the weapon is invalid, return.
            if (!attacker.IsValid() || !item.IsValid() || !comp.RestrictMelee)
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
