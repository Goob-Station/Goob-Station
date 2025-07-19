// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.Keyring;
using Content.Server.DoAfter;
using Content.Server.Doors.Systems;
using Content.Server.Popups;
using Content.Shared.Access.Components;
using Content.Shared.DoAfter;
using Content.Shared.Doors.Components;
using Content.Shared.Interaction;
using Robust.Server.Audio;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Keyring;

public sealed class KeyringSystem : EntitySystem
{
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly DoorSystem _doorSystem = default!;
    [Dependency] private readonly AudioSystem _audiosystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<KeyringComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<KeyringComponent, AfterInteractEvent>(OnInteractUsing);
        SubscribeLocalEvent<KeyringComponent, KeyringDoAfterEvent>(OnDoAfterEvent);
    }

    private void OnInit(Entity<KeyringComponent> keyring, ref MapInitEvent mapInitEvent)
    {
        var access = EnsureComp<AccessComponent>(keyring);

        for (var i = 0; i < keyring.Comp.MaxPossibleAccesses; i++)
        {
            var pick = _random.PickAndTake(keyring.Comp.PossibleAccesses.ToList());
            access.Tags.Add(pick);
        }

    }
    private void OnInteractUsing(Entity<KeyringComponent> keyring, ref AfterInteractEvent args)
    {
        if (args.Handled
            || !HasComp<DoorComponent>(args.Target))
            return;

        var doAfterArgs =
            new DoAfterArgs(EntityManager,
                args.User,
                keyring.Comp.UnlockAttemptDuration,
                new KeyringDoAfterEvent(),
                keyring,
                args.Target)
            {
                BlockDuplicate = true,
                BreakOnMove = true,
                BreakOnHandChange = true,
                BreakOnDamage = true,
            };

        _doAfter.TryStartDoAfter(doAfterArgs);

        var popup = Loc.GetString("keyring-start-unlock-popup");
        _popupSystem.PopupEntity(popup, args.User, args.User);

        _audiosystem.PlayPvs(keyring.Comp.UseSound, keyring);

        args.Handled = true;
    }

    private void OnDoAfterEvent(Entity<KeyringComponent> keyring, ref KeyringDoAfterEvent args)
    {
        if (args.Handled
            || args.Cancelled
            || args.Target is not { } target)
            return;

        if (_doorSystem.TryOpen(target, user: keyring))
        {
            var successPopup = Loc.GetString("keyring-finish-unlock-popup");
            _popupSystem.PopupEntity(successPopup, args.User, args.User);

            return;
        }


        var failPopup = Loc.GetString("keyring-unlock-fail-popup");
        _popupSystem.PopupEntity(failPopup, args.User, args.User);
    }

}
