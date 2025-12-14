using Content.Goobstation.Common.SecondSkin;
using Content.Goobstation.Shared.SecondSkin;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.Alert;
using Content.Shared.Damage.Components;
using Content.Shared.EntityEffects;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Rejuvenate;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.SecondSkin;

public sealed class DisgustSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly AlertsSystem _alets = default!;
    [Dependency] private readonly EntityEffectSystem _effect = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DisgustComponent, RejuvenateEvent>(OnRejuvenate);
        SubscribeLocalEvent<DisgustComponent, ModifyDisgustEvent>(OnModify);
    }

    private void OnModify(Entity<DisgustComponent> ent, ref ModifyDisgustEvent args)
    {
        UpdateLevel(ent, args.Delta);
    }

    private void OnRejuvenate(Entity<DisgustComponent> ent, ref RejuvenateEvent args)
    {
        ent.Comp.Level = 0f;
        UpdateAlert(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var siliconQuery = GetEntityQuery<SiliconComponent>();
        var godmodeQuery = GetEntityQuery<GodmodeComponent>();

        var query = EntityQueryEnumerator<DisgustComponent, MobStateComponent>();
        while (query.MoveNext(out var uid, out var disgust, out var mobstete))
        {
            disgust.Accumulator += frameTime;

            if (disgust.Accumulator < disgust.UpdateTime)
                continue;

            disgust.Accumulator -= disgust.UpdateTime;

            if (siliconQuery.HasComp(uid) || godmodeQuery.HasComp(uid))
            {
                disgust.Level = 0f;
                continue;
            }

            if (mobstete.CurrentState == MobState.Dead)
                continue;

            var ev = new AccumulateDisgustEvent();
            RaiseLocalEvent(uid, ref ev);
            var delta = ev.LevelIncrease;
            if (delta <= 0f)
                delta -= disgust.ReductionRate;

            UpdateLevel((uid, disgust), delta);
            ApplyEffects((uid, disgust));
            UpdateAlert((uid, disgust));
        }
    }

    public void UpdateLevel(Entity<DisgustComponent> ent, float delta)
    {
        if (delta > 0f)
            delta *= ent.Comp.AccumulationMultiplier;
        if (delta == 0f)
            return;
        ent.Comp.Level = Math.Max(0f, ent.Comp.Level + delta);
    }

    private void ApplyEffects(Entity<DisgustComponent> ent)
    {
        if (ent.Comp.Level <= 0f)
            return;

        var args = new EntityEffectBaseArgs(ent, EntityManager);
        foreach (var (level, effects) in ent.Comp.EffectsThresholds)
        {
            if (ent.Comp.Level < level)
                continue;

            foreach (var effect in effects)
            {
                if (!effect.ShouldApply(args, _random))
                    break; // If one of the effects cant be applied, then the rest of them are not applied

                _effect.Effect(effect, args);
            }
        }
    }

    public void UpdateAlert(Entity<DisgustComponent> ent)
    {
        if (ent.Comp.Level <= 0f)
        {
            _alets.ClearAlert(ent, ent.Comp.Alert);
            return;
        }

        short severity = 0;
        foreach (var (level, s) in ent.Comp.SeverityLevels)
        {
            if (ent.Comp.Level >= level)
                severity = Math.Max(severity, s);
        }

        if (severity == 0)
            _alets.ClearAlert(ent, ent.Comp.Alert);
        else
            _alets.ShowAlert(ent, ent.Comp.Alert, severity);
    }
}
