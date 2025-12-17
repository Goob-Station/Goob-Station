using System.Linq;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Heretic;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Shared._Shitcode.Heretic.Systems;

public sealed class CosmosComboSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;

    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmosComboComponent, ComponentShutdown>(OnShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_net.IsClient)
            return;

        var query = EntityQueryEnumerator<CosmosComboComponent>();
        while (query.MoveNext(out var uid, out var combo))
        {
            combo.ComboTimer -= frameTime;

            if (combo.ComboTimer < 0)
                RemCompDeferred(uid, combo);
        }
    }

    private void OnShutdown(Entity<CosmosComboComponent> ent, ref ComponentShutdown args)
    {
        if (!TerminatingOrDeleted(ent))
            RemComp<CosmicTrailComponent>(ent);
    }

    // Hell
    public void ComboProgress(Entity<HereticComponent> uid, IReadOnlyList<EntityUid> hitEntities)
    {
        if (_net.IsClient)
            return;

        var hitAliveMobs = false;
        HashSet<EntityUid> hitMobs = new();
        foreach (var ent in hitEntities)
        {
            if (!TryComp(ent, out MobStateComponent? mobState))
                continue;

            hitAliveMobs |= mobState.CurrentState != MobState.Dead;

            hitMobs.Add(ent);
        }

        if (hitMobs.Count == 0 || hitMobs.Contains(uid))
        {
            RemCompDeferred<CosmosComboComponent>(uid);
            return;
        }

        if (!TryComp(uid, out CosmosComboComponent? combo))
        {
            var dict = hitMobs.ToDictionary(x => x, _ => 1);
            combo = AddComp<CosmosComboComponent>(uid);
            combo.HitEntities = dict;
            combo.ComboCounter = hitAliveMobs ? 1 : 0;
            return;
        }

        if (hitMobs.All(x => combo.HitEntities.ContainsKey(x)))
        {
            RemCompDeferred(uid, combo);
            return;
        }

        if (uid.Comp.Ascended)
        {
            combo.ComboIncreaseTime = 3f;
            combo.MaxComboDuration = 30f;
            combo.ComboDuration = 10f;
            combo.ComboTimer = 10f;
        }

        if (hitAliveMobs)
            combo.ComboCounter++;

        foreach (var hit in hitMobs)
        {
            combo.HitEntities[hit] = 0;
        }

        foreach (var dictHit in combo.HitEntities)
        {
            if (!Exists(dictHit.Key))
                continue;

            switch (dictHit.Value)
            {
                case 1:
                    _damageable.TryChangeDamage(dictHit.Key,
                        combo.DamageToSecondTargets,
                        origin: uid,
                        targetPart: TargetBodyPart.Chest,
                        canMiss: false);
                    _audio.PlayPvs(combo.Sound, dictHit.Key);
                    Spawn(combo.SecondTargetEffect, Transform(dictHit.Key).Coordinates);
                    break;
                case 2:
                    _damageable.TryChangeDamage(dictHit.Key,
                        combo.DamageToThirdTargets,
                        origin: uid,
                        targetPart: TargetBodyPart.Chest,
                        canMiss: false);
                    _audio.PlayPvs(combo.Sound, dictHit.Key);
                    Spawn(combo.ThirdTargetEffect, Transform(dictHit.Key).Coordinates);
                    break;
                case > 2:
                    continue;
            }

            combo.HitEntities[dictHit.Key] = dictHit.Value + 1;
        }

        combo.HitEntities = combo.HitEntities.Where(x => x.Value < 3 && Exists(x.Key)).ToDictionary();

        combo.ComboDuration = MathF.Min(combo.ComboDuration + combo.ComboIncreaseTime, combo.MaxComboDuration);
        combo.ComboTimer = combo.ComboDuration;

        if (combo.ComboCounter >= 3)
            EnsureComp<CosmicTrailComponent>(uid).Strength = uid.Comp.PathStage;
    }
}
