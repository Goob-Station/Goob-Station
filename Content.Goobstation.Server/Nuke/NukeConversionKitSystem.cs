// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Nuke;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Nuke;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Server.Nuke;

public sealed partial class NukeConversionKitSystem : EntitySystem
{
    [Dependency] SharedDoAfterSystem _doAfter = default!;
    [Dependency] SharedPopupSystem _popups = default!;
    [Dependency] NukeSystem _nuke = default!;
    [Dependency] SharedTransformSystem _transform = default!;
    [Dependency] SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NukeConversionKitComponent, AfterInteractEvent>(OnKitAfterInteract);
        SubscribeLocalEvent<NukeConversionKitComponent, NukeConversionDoAfterEvent>(OnKitDoAfter);
    }

    #region Conversion Kit

    private void OnKitAfterInteract(Entity<NukeConversionKitComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target is not { } target)
            return;

        if (!TryComp<NukeComponent>(target, out var nuke))
            return;

        args.Handled = true;

        if (nuke.Converted)
        {
            _popups.PopupClient(Loc.GetString("nuke-conversion-already-converted"), target, args.User);
            return;
        }

        if (nuke.Status == NukeStatus.ARMED)
        {
            _popups.PopupClient(Loc.GetString("nuke-conversion-armed"), target, args.User, PopupType.MediumCaution);
            return;
        }

        var doAfter = new DoAfterArgs(EntityManager, args.User, nuke.ConversionDoAfterLength, new NukeConversionDoAfterEvent(), ent.Owner, target: target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = true,
            MultiplyDelay = false,
        };

        if (_doAfter.TryStartDoAfter(doAfter))
            _popups.PopupClient(Loc.GetString("nuke-conversion-start"), target, args.User);
    }

    private void OnKitDoAfter(Entity<NukeConversionKitComponent> ent, ref NukeConversionDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target is not { } target)
            return;

        if (!TryComp<NukeComponent>(target, out var nuke) || nuke.Converted || nuke.Status == NukeStatus.ARMED)
            return;

        args.Handled = true;

        _nuke.ConvertBomb(target, nuke);
        _audio.PlayPvs(ent.Comp.ConversionSound, target);
        _popups.PopupCoordinates(Loc.GetString("nuke-conversion-finished"), _transform.GetMoverCoordinates(target), PopupType.Large);
        QueueDel(ent.Owner);
    }

    #endregion
}
