using Content.Shared._Goobstation.Heretic.Systems;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Effects;
using Content.Shared.Examine;
using Content.Shared.Ghost;
using Content.Shared.Heretic;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Shared._Shitcode.Heretic.Systems;

public sealed class MirrorMaidSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly MobThresholdSystem _threshold = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedVoidCurseSystem _curse = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MirrorMaidComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<MirrorMaidComponent, MeleeHitEvent>(OnHit);
    }

    private void OnHit(Entity<MirrorMaidComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit)
            return;

        foreach (var hit in args.HitEntities)
        {
            _curse.DoCurse(hit);
        }
    }

    private void OnExamine(Entity<MirrorMaidComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.ExamineDamage.Empty || args.Examiner == ent.Owner ||
            HasComp<GhostComponent>(args.Examiner) || HasComp<SpectralComponent>(args.Examiner) ||
            HasComp<GhostComponent>(args.Examiner) || HasComp<HereticComponent>(args.Examiner) ||
            HasComp<MirrorMaidComponent>(args.Examiner) ||
            _status.HasStatusEffect(args.Examiner, ent.Comp.ExamineStatus))
            return;

        if (!_threshold.TryGetThresholdForState(ent, MobState.Critical, out var threshold) ||
            !_threshold.TryGetThresholdForState(ent, MobState.Dead, out threshold))
            return;

        if (_damageable.TryChangeDamage(ent,
                ent.Comp.ExamineDamage,
                true,
                origin: args.Examiner,
                targetPart: TargetBodyPart.Vital) == null)
            return;

        _status.TryUpdateStatusEffectDuration(args.Examiner, ent.Comp.ExamineStatus, ent.Comp.ExamineDelay);

        _color.RaiseEffect(Color.White.WithAlpha(0.5f),
            new() { ent },
            Filter.Pvs(ent).RemovePlayerByAttachedEntity(args.Examiner),
            0.5f);

        _popup.PopupClient(Loc.GetString("mirror-maid-examine-message-user",
                ("ent", Identity.Entity(ent, EntityManager, args.Examiner))),
            ent,
            args.Examiner);
        _popup.PopupEntity(Loc.GetString("mirror-maid-examine-message-maid",
                ("user", Identity.Entity(args.Examiner, EntityManager, ent))),
            ent,
            ent,
            PopupType.MediumCaution);

        _audio.PlayPredicted(ent.Comp.ExamineSound, ent, args.Examiner);
    }
}
