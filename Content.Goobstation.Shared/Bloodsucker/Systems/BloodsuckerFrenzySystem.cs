using Content.Goobstation.Common.Traits;
using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Goobstation.Shared.Bloodsuckers.Events;
using Content.Shared.Body.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Speech.Muting;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Bloodsuckers.Systems;

/// <summary>
/// Monitors the vampire's blood volume.
/// </summary>
public sealed class BloodsuckerFrenzySystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly BloodsuckerHumanitySystem _humanity = default!;

    private static readonly ProtoId<DamageTypePrototype> BurnDamageType = "Burn";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodsuckerFrenzyComponent, ComponentInit>(OnFrenzyInit);
        SubscribeLocalEvent<BloodsuckerFrenzyComponent, ComponentRemove>(OnFrenzyRemoved);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Check whether any non-frenzied bloodsucker should enter frenzy.
        var bloodsuckerQuery = EntityQueryEnumerator<BloodsuckerComponent, BloodstreamComponent>();
        while (bloodsuckerQuery.MoveNext(out var uid, out var sucker, out var bloodstream))
        {
            if (HasComp<BloodsuckerFrenzyComponent>(uid))
                continue;

            var currentBlood = GetBloodVolume(bloodstream);
            if (currentBlood <= sucker.FrenzyThreshold)
                EnterFrenzy(uid, sucker);
        }

        // Tick burn DoT on frenzied bloodsuckers.
        var frenzyQuery = EntityQueryEnumerator<BloodsuckerFrenzyComponent, BloodsuckerComponent, BloodstreamComponent>();
        while (frenzyQuery.MoveNext(out var uid, out var frenzy, out var sucker, out var bloodstream))
        {
            var currentBlood = GetBloodVolume(bloodstream);
            if (currentBlood >= sucker.FrenzyExitThreshold)
            {
                ExitFrenzy(uid);
                continue;
            }

            frenzy.BurnAccumulator += frameTime;
            while (frenzy.BurnAccumulator >= frenzy.BurnTickRate)
            {
                frenzy.BurnAccumulator -= frenzy.BurnTickRate;
                ApplyFrenzyBurn(uid, frenzy);
            }
        }
    }

    private void EnterFrenzy(EntityUid uid, BloodsuckerComponent sucker)
    {
        float burnDps = 3f;
        if (TryComp(uid, out BloodsuckerHumanityComponent? humanity))
            burnDps = _humanity.GetFrenzyBurnDamage(new Entity<BloodsuckerHumanityComponent>(uid, humanity));

        var frenzy = AddComp<BloodsuckerFrenzyComponent>(uid);
        frenzy.BurnDamagePerSecond = burnDps;

        ApplySensoryDebuffs(uid, frenzy);

        RaiseLocalEvent(uid, new BloodsuckerFrenzyEnteredEvent { BurnDamagePerSecond = burnDps });
        Dirty(uid, frenzy);
    }

    private void ExitFrenzy(EntityUid uid)
    {
        if (!TryComp(uid, out BloodsuckerFrenzyComponent? frenzy))
            return;

        RemoveSensoryDebuffs(uid, frenzy);
        RemComp<BloodsuckerFrenzyComponent>(uid);
        RaiseLocalEvent(uid, new BloodsuckerFrenzyExitedEvent());
    }

    private void OnFrenzyInit(Entity<BloodsuckerFrenzyComponent> ent, ref ComponentInit args)
    {
        if (!ent.Comp.DeafnessApplied || !ent.Comp.MutenessApplied)
            ApplySensoryDebuffs(ent, ent.Comp);
    }

    private void OnFrenzyRemoved(Entity<BloodsuckerFrenzyComponent> ent, ref ComponentRemove args)
    {
        RemoveSensoryDebuffs(ent, ent.Comp);
    }

    private static float GetBloodVolume(BloodstreamComponent bloodstream)
=> bloodstream.BloodSolution is { } sol
    ? (float) sol.Comp.Solution.Volume
    : 0f;

    private void ApplyFrenzyBurn(EntityUid uid, BloodsuckerFrenzyComponent frenzy)
    {
        var damage = new DamageSpecifier();
        damage.DamageDict[BurnDamageType] = frenzy.BurnDamagePerSecond * frenzy.BurnTickRate;
        _damageable.TryChangeDamage(uid, damage, ignoreResistances: false, interruptsDoAfters: false);
    }

    private void ApplySensoryDebuffs(EntityUid uid, BloodsuckerFrenzyComponent frenzy)
    {
        if (!frenzy.DeafnessApplied)
        {
            EnsureComp<DeafComponent>(uid);
            frenzy.DeafnessApplied = true;
        }

        if (!frenzy.MutenessApplied)
        {
            EnsureComp<MutedComponent>(uid);
            frenzy.MutenessApplied = true;
        }
    }
    private void RemoveSensoryDebuffs(EntityUid uid, BloodsuckerFrenzyComponent frenzy)
    {
        if (frenzy.DeafnessApplied)
        {
            RemComp<DeafComponent>(uid);
            frenzy.DeafnessApplied = false;
        }

        if (frenzy.MutenessApplied)
        {
            RemComp<MutedComponent>(uid);
            frenzy.MutenessApplied = false;
        }
    }
}
