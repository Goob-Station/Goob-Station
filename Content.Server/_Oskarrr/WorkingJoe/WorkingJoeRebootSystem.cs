// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Oskarrr.WorkingJoe;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Utility;

namespace Content.Server._Oskarrr.WorkingJoe;

/// <summary>
/// Reboot dead Working Joe / Jockey units with the Seegson reset key when damage is cleared.
/// </summary>
public sealed class WorkingJoeRebootSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private static readonly SoundPathSpecifier ChargeSound =
        new("/Audio/Machines/button.ogg", AudioParams.Default.WithVolume(-4));

    private static readonly SoundPathSpecifier SuccessSound =
        new("/Audio/Machines/scan_complete.ogg", AudioParams.Default.WithVolume(-2));

    private static readonly SoundPathSpecifier FailSound =
        new("/Audio/Machines/buzz-sigh.ogg", AudioParams.Default.WithVolume(-2));

    private const float DoAfterSeconds = 5f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WorkingJoeVoiceComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
        SubscribeLocalEvent<WorkingJoeVoiceComponent, WorkingJoeRebootDoAfterEvent>(OnRebootDoAfter);
        SubscribeLocalEvent<WorkingJoeSynthResetKeyComponent, AfterInteractEvent>(OnKeyAfterInteract);
    }

    private void OnGetVerbs(EntityUid uid, WorkingJoeVoiceComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (!HasResetKey(args.User))
            return;

        if (!CanReboot(uid, out _))
            return;

        var user = args.User;
        var target = uid;

        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("working-joe-reboot-verb"),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/settings.svg.192dpi.png")),
            Act = () => TryStartReboot(user, target),
            Priority = 2
        });
    }

    private void OnKeyAfterInteract(EntityUid uid, WorkingJoeSynthResetKeyComponent component, AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target is not { } target)
            return;

        if (!HasComp<WorkingJoeVoiceComponent>(target))
            return;

        if (!CanReboot(target, out var reason))
        {
            if (reason != null)
                _popup.PopupEntity(reason, target, args.User);
            return;
        }

        args.Handled = TryStartReboot(args.User, target);
    }

    private bool HasResetKey(EntityUid user)
    {
        foreach (var held in _hands.EnumerateHeld(user))
        {
            if (HasComp<WorkingJoeSynthResetKeyComponent>(held))
                return true;
        }

        return false;
    }

    private bool CanReboot(EntityUid target, out string? reason)
    {
        reason = null;

        if (!_mobState.IsDead(target))
        {
            reason = Loc.GetString("working-joe-reboot-not-dead");
            return false;
        }

        if (!TryComp(target, out DamageableComponent? damageable) || damageable.TotalDamage > 0)
        {
            reason = Loc.GetString("working-joe-reboot-damaged");
            return false;
        }

        return true;
    }

    private bool TryStartReboot(EntityUid user, EntityUid target)
    {
        if (!CanReboot(target, out var reason))
        {
            if (reason != null)
                _popup.PopupEntity(reason, target, user);
            return false;
        }

        var ev = new WorkingJoeRebootDoAfterEvent();
        var args = new DoAfterArgs(EntityManager, user, TimeSpan.FromSeconds(DoAfterSeconds), ev, target, target)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = true
        };

        if (!_doAfter.TryStartDoAfter(args))
            return false;

        _audio.PlayPvs(ChargeSound, target);
        _popup.PopupEntity(Loc.GetString("working-joe-reboot-started"), target, user);
        return true;
    }

    private void OnRebootDoAfter(EntityUid uid, WorkingJoeVoiceComponent component, WorkingJoeRebootDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        args.Handled = true;

        if (!CanReboot(uid, out var reason))
        {
            _audio.PlayPvs(FailSound, uid);
            if (reason != null)
                _popup.PopupEntity(reason, uid, args.User);
            return;
        }

        _mobState.ChangeMobState(uid, MobState.Alive);
        _audio.PlayPvs(SuccessSound, uid);
        _popup.PopupEntity(Loc.GetString("working-joe-reboot-success"), uid, args.User);
    }
}
