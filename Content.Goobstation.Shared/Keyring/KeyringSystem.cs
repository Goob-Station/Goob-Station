// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.Keyring;

public sealed class KeyringSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedDoorSystem _doorSystem = default!;
    [Dependency] private readonly AccessReaderSystem _access = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
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
        if (keyring.Comp.PossibleAccesses.Count == 0)
            return;

        for (var i = 0; i < keyring.Comp.MaxPossibleAccesses; i++)
        {
            var pick = _random.PickAndTake(keyring.Comp.PossibleAccesses.ToList());
            keyring.Comp.Tags.Add(pick); // We don't use access comp for this because otherwise you can use it like an ID card to bump open doors :P
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
        _popupSystem.PopupClient(popup, args.User, args.User);

        _audioSystem.PlayPredicted(keyring.Comp.UseSound, keyring, args.User);

        args.Handled = true;
    }

    private void OnDoAfterEvent(Entity<KeyringComponent> keyring, ref KeyringDoAfterEvent args)
    {
        if (args.Handled
            || args.Cancelled
            || args.Target is not { } target
            || !_access.GetMainAccessReader(target, out var accessReader))
            return;

        if (_access.AreAccessTagsAllowed(keyring.Comp.Tags, accessReader))
        {
            _doorSystem.StartOpening(target);

            var successPopup = Loc.GetString("keyring-finish-unlock-popup");
            _popupSystem.PopupClient(successPopup, args.User, args.User);

            return;
        }


        var failPopup = Loc.GetString("keyring-unlock-fail-popup");
        _popupSystem.PopupClient(failPopup, args.User, args.User);
    }

}
