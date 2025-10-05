using System.Linq;
using Content.Goobstation.Common.Physics;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Bed.Sleep;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Heretic;
using Content.Shared.Interaction;
using Content.Shared.Magic;
using Content.Shared.Mobs.Components;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._Shitcode.Heretic.Systems;

public abstract class SharedStarTouchSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly SharedMagicSystem _magic = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedStarMarkSystem _starMark = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public static readonly EntProtoId StarTouchStatusEffect = "StatusEffectStarTouched";
    public static readonly EntProtoId StarTouchBeamDataId = "startouch";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StarTouchComponent, AfterInteractEvent>(OnAfterInteract);

        SubscribeLocalEvent<StarTouchedStatusEffectComponent, StatusEffectAppliedEvent>(OnApply);
        SubscribeLocalEvent<StarTouchedStatusEffectComponent, StatusEffectRemovedEvent>(OnRemove);
    }

    private void OnRemove(Entity<StarTouchedStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (!TerminatingOrDeleted(args.Target) && TryComp(args.Target, out StarTouchedComponent? touch))
            RemCompDeferred(args.Target, touch);
    }

    private void OnApply(Entity<StarTouchedStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        EnsureComp<StarTouchedComponent>(args.Target);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<StarTouchedComponent, DamageableComponent>();
        while (query.MoveNext(out var uid, out var touch, out var dmg))
        {
            touch.Accumulator += frameTime;

            if (touch.Accumulator < touch.TickInterval)
                continue;

            touch.Accumulator = 0f;

            UpdateBeams((uid, touch));

            _damageable.TryChangeDamage(uid,
                touch.Damage,
                damageable: dmg,
                targetPart: TargetBodyPart.Chest,
                canMiss: false);
        }
    }

    private void UpdateBeams(Entity<StarTouchedComponent, ComplexJointVisualsComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp2, false))
            return;

        var hasStarBeams = false;

        foreach (var (netEnt, _) in ent.Comp2.Data.Where(x => x.Value.Id == StarTouchBeamDataId).ToList())
        {
            if (!TryGetEntity(netEnt, out var target) || TerminatingOrDeleted(target) ||
                !_examine.InRangeUnOccluded(target.Value, ent.Owner, ent.Comp1.Range))
            {
                ent.Comp2.Data.Remove(netEnt);
                continue;
            }

            hasStarBeams = true;
        }

        Dirty(ent.Owner, ent.Comp2);

        if (hasStarBeams)
            return;

        _status.TryRemoveStatusEffect(ent, StarTouchStatusEffect);
    }

    private void OnAfterInteract(Entity<StarTouchComponent> ent, ref AfterInteractEvent args)
    {
        if (!args.CanReach)
            return;

        if (args.Target == null || args.Target == args.User)
            return;

        var (uid, comp) = ent;

        var target = args.Target.Value;

        if (!HasComp<MobStateComponent>(target))
            return;

        args.Handled = true;

        if (!TryComp<HereticComponent>(args.User, out var hereticComp) ||
            TryComp<HereticComponent>(target, out var th) && th.CurrentPath == hereticComp.CurrentPath)
        {
            PredictedQueueDel(uid);
            return;
        }

        if (_magic.IsTouchSpellDenied(target))
        {
            InvokeSpell(ent, args.User);
            return;
        }

        var range = hereticComp.Ascended ? 2 : 1;
        _starMark.SpawnCosmicFields(Transform(args.User).Coordinates, range);

        if (HasComp<StarMarkComponent>(target))
        {
            if (_status.TryUpdateStatusEffectDuration(target, SleepingSystem.StatusEffectForcedSleeping, comp.SleepTime))
                _status.TryRemoveStatusEffect(target, SharedStarMarkSystem.StarMarkStatusEffect);
        }
        else
        {
            _status.TryUpdateStatusEffectDuration(target,
                SharedStarMarkSystem.StarMarkStatusEffect,
                TimeSpan.FromSeconds(30));
        }

        if (_status.TryUpdateStatusEffectDuration(target, StarTouchStatusEffect, ent.Comp.Duration))
        {
            var beam = EnsureComp<ComplexJointVisualsComponent>(target);
            beam.Data[GetNetEntity(args.User)] = new ComplexJointVisualsData(StarTouchBeamDataId, ent.Comp.BeamSprite);
            Dirty(target, beam);
        }

        InvokeSpell(ent, args.User);
    }

    public virtual void InvokeSpell(Entity<StarTouchComponent> ent, EntityUid user, bool deleteSpell = true)
    {
        _audio.PlayPredicted(ent.Comp.Sound, user, user);
    }
}
