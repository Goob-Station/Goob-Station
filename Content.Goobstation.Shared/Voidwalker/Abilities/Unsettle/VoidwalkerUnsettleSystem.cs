using Content.Goobstation.Shared.SpecialAnimation;
using Content.Goobstation.Shared.Voidwalker;
using Content.Goobstation.Shared.Voidwalker.Abilities.Unsettle;
using Content.Goobstation.Shared.Voidwalker.Actions;
using Content.Shared.Chat;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.Stunnable;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

public sealed partial class VoidwalkerUnsettleSystem : EntitySystem
{
    [Dependency] private readonly SharedVoidwalkerSystem _voidwalker = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedChatSystem _chat = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly SharedSlurredSystem _slurred = default!;
    [Dependency] private readonly SharedSpecialAnimationSystem _specialAnimation = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<VoidwalkerUnsettleComponent, VoidwalkerUnsettleEvent>(OnUnsettle);
        SubscribeLocalEvent<VoidwalkerUnsettleComponent, VoidwalkerUnsettleDoAfterEvent>(OnUnsettleDoAfter);

        SubscribeLocalEvent<VoidwalkerUnsettleComponent, ExaminedEvent>(OnExamined);
    }

    private void OnUnsettle(Entity<VoidwalkerUnsettleComponent> entity, ref VoidwalkerUnsettleEvent args)
    {
        var target = args.Target;

        if (!_voidwalker.TryUseAbility(entity, args))
            return;

        if (_mobState.IsIncapacitated(target))
        {
            _popup.PopupClient(Loc.GetString("voidwalker-unsettle-fail-incapacitated"), target, entity);
            return;
        }

        if (!_voidwalker.CanSeeVoidwalker(target))
        {
            _popup.PopupClient(Loc.GetString("voidwalker-unsettle-fail-blind"), target, entity);
            return;
        }

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            entity,
            entity.Comp.UnsettleDoAfterDuration,
            new VoidwalkerUnsettleDoAfterEvent(),
            eventTarget: entity,
            target: target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            RequireCanInteract = false, // use your EYES!!!!
            DistanceThreshold = 20,
            IgnoreObstruction = true,
            Hidden = true,
        };

        if (!_doAfter.TryStartDoAfter(doAfterArgs, out var id))
            return;

        entity.Comp.UnsettleDoAfterId = id.Value.Index;

        var popup = Loc.GetString("voidwalker-unsettle-begin", ("target", Name(target)));
        _popup.PopupEntity(popup, entity, entity, PopupType.Medium);
    }

    private void OnUnsettleDoAfter(Entity<VoidwalkerUnsettleComponent> entity, ref VoidwalkerUnsettleDoAfterEvent args)
    {
        entity.Comp.UnsettleDoAfterId = null;

        if (args.Target is not { } target
            || args.Cancelled
            || args.Handled)
            return;

        args.Handled = true;

        _stun.KnockdownOrStun(target, entity.Comp.UnsettleStunDuration, true);
        _chat.TryEmoteWithChat(target, entity.Comp.ScreamProtoId);
        _stamina.TakeStaminaDamage(target, entity.Comp.UnsettleStaminaDamage);
        _slurred.DoSlur(target, entity.Comp.UnsettleStunDuration * 2);

        var popup = Loc.GetString("voidwalker-unsettle-victim");
        _popup.PopupClient(popup, target, target, PopupType.LargeCaution);
        _specialAnimation.PlayAnimationForEntity(entity.Comp.JumpscareSprite, target, entity.Comp.JumpscarePrototype);
        _audio.PlayEntity(entity.Comp.JumpscareSound, target, target, AudioParams.Default);
    }

    private void OnExamined(Entity<VoidwalkerUnsettleComponent> entity, ref ExaminedEvent args)
    {
        if (entity.Comp.UnsettleDoAfterId is not { } doAfterId)
            return;

        _doAfter.Cancel(entity, doAfterId);
        entity.Comp.UnsettleDoAfterId = null;

        var popup = Loc.GetString("voidwalker-unsettle-fail-looked-at");
        _popup.PopupClient(popup, entity, entity, PopupType.MediumCaution);
    }
}
