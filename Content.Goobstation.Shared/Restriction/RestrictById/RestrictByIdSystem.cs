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
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Restriction.RestrictById
{
    public sealed partial class RestrictByIdSystem : EntitySystem
    {
        [Dependency] private readonly AccessReaderSystem _accessReader = default!;
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
        [Dependency] private readonly EmagSystem _emag = default!;
        [Dependency] private readonly IGameTiming _timing = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<RestrictByIdComponent, MapInitEvent>(OnComponentInit);
            SubscribeLocalEvent<RestrictByIdComponent, AttemptShootEvent>(OnAttemptShoot);
            SubscribeLocalEvent<RestrictByIdComponent, AttemptMeleeEvent>(OnAttemptMelee);
            SubscribeLocalEvent<RestrictByIdComponent, GotEmaggedEvent>(OnEmagged);
        }

        private void OnComponentInit(Entity<RestrictByIdComponent> ent, ref MapInitEvent args)
        {
            EnsureComp<AccessReaderComponent>(ent, out var accessReader);
            _accessReader.SetAccesses(ent, accessReader, ent.Comp.AccessLists);
        }

        private void OnEmagged(Entity<RestrictByIdComponent> ent, ref GotEmaggedEvent args)
        {
            if (!_emag.CompareFlag(args.Type, EmagType.Interaction) || !ent.Comp.IsEmaggable)
                return;

            RemComp<AccessReaderComponent>(ent);
            ent.Comp.IsEmagged = true;
            args.Handled = true;
            args.Repeatable = false;
        }

        private void OnAttemptShoot(Entity<RestrictByIdComponent> ent, ref AttemptShootEvent args)
        {
            if (ent.Comp.IsEmagged || !ent.Comp.RestrictRanged)
                return;

            if (_accessReader.IsAllowed(args.User, ent))
                return;

            args.Cancelled = true;
            args.Message = Loc.GetString(ent.Comp.FailText);
        }

        private void OnAttemptMelee(Entity<RestrictByIdComponent> ent, ref AttemptMeleeEvent args)
        {
            if (ent.Comp.IsEmagged || !ent.Comp.RestrictMelee)
                return;

            if (_accessReader.IsAllowed(args.User, ent))
                return;

            args.Cancelled = true;
            args.Message = Loc.GetString(ent.Comp.FailText);
        }
    }
}
