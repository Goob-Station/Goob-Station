using Content.Goobstation.Shared.Cyberpsychosis;
using Content.Goobstation.Shared.Cyberware;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Cyberpsychosis;

/// <summary>
/// Handles the Sanity drain / meter for cybernetics. Placeholder for now.
/// </summary>
public sealed class CyberSanitySystem : EntitySystem
{
    [Dependency] private readonly CyberneticsSystem _cybernetics = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CyberSanityComponent, CollectCyberSanityThresholdsEvent>(OnCollectThresholds);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;

        var query = EntityQueryEnumerator<CyberSanityComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (now < comp.NextUpdate)
                continue;

            comp.NextUpdate = now + comp.UpdateInterval;

            if (_mobState.IsIncapacitated(uid))
                continue;

            UpdateSanity(uid, comp, now);
        }
    }

    private void UpdateSanity(EntityUid uid, CyberSanityComponent comp, TimeSpan now)
    {
        var decay = TotalDrain(uid);
        var delta = comp.Regen - decay;

        comp.Sanity = Math.Clamp(comp.Sanity + delta, 0f, comp.MaxSanity);
        comp.Decay = decay;
        comp.Rate = delta;

        ApplyEffects(uid, comp, now);
    }

    /// <summary>
    /// Total cyber sanity drain.
    /// </summary>
    private float TotalDrain(EntityUid body)
    {
        if (!_cybernetics.TryGetImplants(body, out var implants))
            return 0f;

        var total = 0f;
        foreach (var implant in implants)
            total += ImplantDrain(implant.Comp);

        return total;
    }

    /// <summary>
    /// Cyber sanity drain per implant.
    /// </summary>
    public static float ImplantDrain(CyberneticsComponent ware)
    {
        if (!ware.Enabled)
            return 0f;

        var drain = ware.Drain ?? 0f;

        if (ware.Active && ware.ActiveDrain is { } active)
            drain = active;

        if (ware.Overloaded && ware.OverloadDrain is { } overload)
            drain += overload;

        return drain * ClockDrainMultiplier(ware);
    }

    public static float ClockDrainMultiplier(CyberneticsComponent ware)
        => ware.ClockDrainMultipliers.GetValueOrDefault(ware.ClockStep, 1f);

    private void ApplyEffects(EntityUid uid, CyberSanityComponent comp, TimeSpan now)
    {
        var rollSymptom = now >= comp.NextSymptom;
        var pool = RunContinuousAndPoolSymptoms(uid, comp, rollSymptom);

        if (!rollSymptom || pool.Count == 0)
            return;

        FireSymptom(uid, comp, now, _random.Pick(pool));
    }

    private List<CyberSanityEffect> RunContinuousAndPoolSymptoms(
        EntityUid uid,
        CyberSanityComponent comp,
        bool rollSymptom)
    {
        var pool = new List<CyberSanityEffect>();

        foreach (var tier in comp.Tiers.Values)
        {
            var breached = comp.Sanity <= tier.Threshold;
            foreach (var effect in tier.Effects)
            {
                if (effect.Continuous)
                    effect.Effect(uid, comp, EntityManager, _random, tier.Threshold);
                else if (breached && rollSymptom)
                    pool.Add(effect);
            }
        }

        return pool;
    }

    private void FireSymptom(EntityUid uid, CyberSanityComponent comp, TimeSpan now, CyberSanityEffect effect)
    {
        comp.NextSymptom = now + TimeSpan.FromSeconds(_random.NextFloat(effect.DelayMin, effect.DelayMax));
        effect.Effect(uid, comp, EntityManager, _random, 0f);
    }

    private void OnCollectThresholds(Entity<CyberSanityComponent> ent, ref CollectCyberSanityThresholdsEvent args)
    {
        foreach (var (symptom, tier) in ent.Comp.Tiers)
        {
            var key = symptom.ToString().ToLowerInvariant();
            args.Thresholds.Add(new CyberSanityThresholdEntry
            {
                Value = tier.Threshold,
                Label = Loc.GetString($"cyberware-threshold-{key}"),
                Description = Loc.GetString($"cyberware-threshold-{key}-desc", ("value", (int) tier.Threshold)),
            });
        }
    }

    /// <summary>
    /// This will be used later for the UI.
    /// TODO: Cyberpsychosis, update it to show jack-in sanity cost.
    /// </summary>
    public void ChangeSanity(Entity<CyberSanityComponent?> ent, float delta)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        ent.Comp.Sanity = Math.Clamp(ent.Comp.Sanity + delta, 0f, ent.Comp.MaxSanity);
    }
}
