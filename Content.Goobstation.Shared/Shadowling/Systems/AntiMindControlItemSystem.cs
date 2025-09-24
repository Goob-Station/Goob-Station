using Content.Goobstation.Shared.Shadowling.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.Shadowling.Systems;

/// <summary>
/// This handles the Anti-Mind control item system
/// Its the same as enthralling, but takes some seconds more.
/// Has charges, and is limited through crafting it
/// </summary>
public sealed class AntiMindControlItemSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedChargesSystem _charges = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AntiMindControlItemComponent, AfterInteractEvent>(AfterInteract);
        SubscribeLocalEvent<AntiMindControlItemComponent, AntiMindControlItemDoAfterEvent>(AntiMindControlDoAfter);

        SubscribeLocalEvent<AntiMindControlItemComponent, ExaminedEvent>(AntiMindControlExamined);
    }

    private void AntiMindControlExamined(EntityUid uid, AntiMindControlItemComponent component, ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("anti-mind-examine-charges", ("charges",  _charges.GetCurrentCharges(uid))));
    }

    private void AfterInteract(EntityUid uid, AntiMindControlItemComponent component, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach)
            return;

        if (!_charges.HasCharges(uid, 1))
        {
            _popupSystem.PopupPredicted(
                Loc.GetString("anti-mind-max-charges-reached"),
                args.User,
                args.User,
                PopupType.MediumCaution);
            return;
        }

        if (args.Target == null)
            return;

        var target = args.Target.Value;

        if (args.User == target
            || !HasComp<HumanoidAppearanceComponent>(target))
            return;

        var doAfter = new DoAfterArgs(
            EntityManager,
            args.User,
            component.Duration,
            new AntiMindControlItemDoAfterEvent(),
            uid,
            target,
            args.Used)
        {
            CancelDuplicate = true,
            BreakOnDamage = true,
            NeedHand = true,
            BreakOnHandChange = true,
        };

        _doAfter.TryStartDoAfter(doAfter);
        args.Handled = true;
    }

    private void AntiMindControlDoAfter(EntityUid uid, AntiMindControlItemComponent component, AntiMindControlItemDoAfterEvent args)
    {
        if (args.Used == null
            || args.Cancelled
            || args.Handled
            || args.Args.Target == null
            || !_charges.TryUseCharge(uid))
            return;

        var target = args.Args.Target.Value;
        var user = args.Args.User;

        if (HasComp<LesserShadowlingComponent>(target))
        {
            _popupSystem.PopupPredicted(Loc.GetString("mind-control-lesser-shadowling"), user, user, PopupType.MediumCaution);
            return;
        }

        if (HasComp<ThrallComponent>(target))
        {
            RemComp<ThrallComponent>(target);

            if (!HasComp<EnthrallResistanceComponent>(target))
                EnsureComp<EnthrallResistanceComponent>(target);

            var enthrallRes = EntityManager.GetComponent<EnthrallResistanceComponent>(target);
            enthrallRes.ExtraTime += enthrallRes.ExtraTimeUpdate;

            _popupSystem.PopupPredicted(Loc.GetString("mind-control-thrall-done"), target, target, PopupType.MediumCaution);
        }

        _audioSystem.PlayPredicted(
            new SoundPathSpecifier("/Audio/Weapons/flash.ogg"),
            user,
            target,
            AudioParams.Default.WithVolume(-2f));

        args.Handled = true;
    }
}
