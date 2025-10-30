using Content.Goobstation.Common.Traits;
using Content.Server.Atmos.Components;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Curses;
using Content.Shared._Shitmed.Damage;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;

namespace Content.Server._Shitcode.Heretic.Curses;

public sealed partial class HereticCurseSystem
{
    private void InitializeCurses()
    {
        SubscribeLocalEvent<CurseOfParalysisStatusEffectComponent, StatusEffectAppliedEvent>(OnParalysisApply);
        SubscribeLocalEvent<CurseOfParalysisStatusEffectComponent, StatusEffectRemovedEvent>(OnParalysisRemove);
        SubscribeLocalEvent<CurseOfAmokStatusEffectComponent, StatusEffectAppliedEvent>(OnAmokApply);
        SubscribeLocalEvent<CurseOfAmokStatusEffectComponent, StatusEffectRemovedEvent>(OnAmokRemove);
        SubscribeLocalEvent<CurseOfFragilityStatusEffectComponent, StatusEffectAppliedEvent>(OnFragilityApply);
        SubscribeLocalEvent<CurseOfFragilityStatusEffectComponent, StatusEffectRemovedEvent>(OnFragilityRemove);
        SubscribeLocalEvent<CurseOfBlindnessStatusEffectComponent, StatusEffectAppliedEvent>(OnBlindnessApply);
        SubscribeLocalEvent<CurseOfBlindnessStatusEffectComponent, StatusEffectRemovedEvent>(OnBlindnessRemove);

        SubscribeLocalEvent<FragileComponent, DamageModifyEvent>(OnModify);
    }

    private void OnModify(Entity<FragileComponent> ent, ref DamageModifyEvent args)
    {
        args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage, ent.Comp.ModifierSet);
    }

    private void OnBlindnessApply(Entity<CurseOfBlindnessStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        EnsureComp<TemporaryBlindnessComponent>(args.Target);
    }

    private void OnBlindnessRemove(Entity<CurseOfBlindnessStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (_timing.ApplyingState)
            return;

        if (TerminatingOrDeleted(args.Target))
            return;

        RemCompDeferred<TemporaryBlindnessComponent>(args.Target);
    }

    private void OnFragilityApply(Entity<CurseOfFragilityStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        EnsureComp<FragileComponent>(args.Target);
    }

    private void OnFragilityRemove(Entity<CurseOfFragilityStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (_timing.ApplyingState)
            return;

        if (TerminatingOrDeleted(args.Target))
            return;

        RemCompDeferred<FragileComponent>(args.Target);
    }

    private void OnAmokApply(Entity<CurseOfAmokStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        var affected = EnsureComp<EntropicPlumeAffectedComponent>(args.Target);
        affected.Duration = float.MaxValue;
        affected.Sprite = null;
        Dirty(args.Target, affected);
    }

    private void OnAmokRemove(Entity<CurseOfAmokStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (_timing.ApplyingState)
            return;

        if (TerminatingOrDeleted(args.Target))
            return;

        RemCompDeferred<EntropicPlumeAffectedComponent>(args.Target);
    }

    private void OnParalysisApply(Entity<CurseOfParalysisStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        ent.Comp.WasParalyzed = EntityManager.EnsureComponent<LegsParalyzedComponent>(args.Target, out _);
    }

    private void OnParalysisRemove(Entity<CurseOfParalysisStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (_timing.ApplyingState || ent.Comp.WasParalyzed)
            return;

        if (TerminatingOrDeleted(args.Target))
            return;

        RemCompDeferred<LegsParalyzedComponent>(args.Target);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;

        var corrosionQuery = EntityQueryEnumerator<CurseOfCorrosionStatusEffectComponent, StatusEffectComponent>();
        while (corrosionQuery.MoveNext(out _, out var corrosion, out var status))
        {
            if (corrosion.NextVomit > curTime || status.AppliedTo == null || status.EndEffectTime < curTime)
                continue;

            var next = _random.NextFloat(corrosion.MinMaxSecondsBetweenVomits.X,
                corrosion.MinMaxSecondsBetweenVomits.Y);

            corrosion.NextVomit = curTime + TimeSpan.FromSeconds(next);

            _dmg.TryChangeDamage(status.AppliedTo.Value,
                corrosion.Damage * next,
                true,
                targetPart: TargetBodyPart.All,
                splitDamage: SplitDamageBehavior.SplitEnsureAll);
            _vomit.Vomit(status.AppliedTo.Value);
        }

        var flammableQuery = GetEntityQuery<FlammableComponent>();

        var flamesQuery = EntityQueryEnumerator<CurseOfFlamesStatusEffectComponent, StatusEffectComponent>();
        while (flamesQuery.MoveNext(out _, out var flames, out var status))
        {
            if (flames.NextIgnition > curTime || status.AppliedTo == null || status.EndEffectTime < curTime)
                continue;

            flames.NextIgnition = curTime + flames.Delay;

            if (!flammableQuery.TryComp(status.AppliedTo.Value, out var flam))
                continue;

            if (flam.FireStacks > flames.MinFireStacks && flam is { OnFire: true, IgnoreFireProtection: true })
                continue;

            _flammable.SetFireStacks(status.AppliedTo.Value, MathF.Max(flames.MinFireStacks, flam.FireStacks), flam);
            // TODO: flammable fire penetration instead of ignoring fire prot
            _flammable.Ignite(status.AppliedTo.Value, null, flam, ignoreFireProtection: true);
        }
    }
}
