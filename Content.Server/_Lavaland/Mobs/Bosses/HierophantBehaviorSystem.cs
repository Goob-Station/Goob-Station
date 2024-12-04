using Content.Server._Lavaland.Aggression;
using Content.Server._Lavaland.Mobs.Bosses.Components;
using Content.Server.Audio;
using Content.Server.Destructible;
using Content.Server.Destructible.Thresholds.Triggers;
using Content.Shared.Audio;
using Content.Shared.Damage;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Linq;
using System.Numerics;

namespace Content.Server._Lavaland.Mobs.Bosses;

public sealed partial class HierophantBehaviorSystem : EntitySystem
{
    [Dependency] private readonly AmbientSoundSystem _ambient = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MegafaunaSystem _megafauna = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;

    [ValidatePrototypeId<EntityPrototype>] private const string DamageBoxPrototype = "LavalandHierophantSquare";
    [ValidatePrototypeId<EntityPrototype>] private const string ChaserPrototype = "LavalandHierophantChaser";

    // used in case of spawning multiple chasers, for example
    static readonly float BaseActionDelay = 1f * 1000f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HierophantBossComponent, AttackedEvent>(_megafauna.OnAttacked);
        SubscribeLocalEvent<HierophantBossComponent, DamageThresholdReached>(_megafauna.OnDeath);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = EntityQueryEnumerator<HierophantBossComponent>();
        while (eqe.MoveNext(out var uid, out var comp))
        {
            if (TryComp<AggressiveComponent>(uid, out var aggressors))
            {
                if (aggressors.Aggressors.Count > 0 && !comp.Aggressive)
                    InitBoss((uid, comp));
                else continue;
            }

            if (comp.Aggressive)
            {
                // todo tick all timers
                var cancelToken = comp.CancelToken.Token;


            }
        }
    }

    private void InitBoss(Entity<HierophantBossComponent> ent)
    {
        ent.Comp.Aggressive = true;

        // add ambient
        if (TryComp<AmbientSoundComponent>(ent, out var ambient))
            _ambient.SetAmbience(ent, true, ambient);
    }

    public void DoMajorAttack(Entity<HierophantBossComponent> ent)
    {

    }
    public void DoMinorAttack(Entity<HierophantBossComponent> ent)
    {

    }

    #region Attacks
    public Action? PickAttack(Entity<HierophantBossComponent> ent)
    {
        return null;
    }

    public void DamageArea(Entity<HierophantBossComponent> ent, int range = 1)
    {
        for (int i = 0; i <= range; i++)
        {
            var target = PickTarget(ent);
            if (target == null)
                return;

            SpawnDamageBox((EntityUid) target, i);
            Timer.Delay((int) GetDelay(ent, BaseActionDelay));
        }
    }
    private void SpawnDamageBox(EntityUid relative, int range = 0)
    {
        // spawn on a single tile only
        if (range == 0)
        {
            Spawn(DamageBoxPrototype, Transform(relative).Coordinates);
            return;
        }

        // spawn around an area
        var xform = Transform(relative);

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return;

        var pos = xform.Coordinates.Position;

        var confines = new Box2(pos + new Vector2(-range, -range), pos + new Vector2(range, range));
        var confinesS = new Box2(pos + new Vector2(-range + 1, -range + 1), pos + new Vector2(range - 1, range - 1));

        var box = _map.GetLocalTilesIntersecting(relative, grid, confines).ToList();
        var boxS = _map.GetLocalTilesIntersecting(relative, grid, confinesS).ToList();

        var boxFinal = box.Where(b => !boxS.Contains(b)).ToList();

        foreach (var tile in boxFinal)
            Spawn(DamageBoxPrototype, _map.ToCoordinates(tile));
    }

    public void SpawnChaser(Entity<HierophantBossComponent> ent, int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            var chaser = Spawn(ChaserPrototype, Transform(ent).Coordinates);
            if (TryComp<HierophantChaserComponent>(chaser, out var chasercomp))
                chasercomp.Target = PickTarget(ent);

            Timer.Delay((int) GetDelay(ent, BaseActionDelay));
        }
    }

    public void BlinkRandom(Entity<HierophantBossComponent> ent, EntityCoordinates? relativePos, int spread = 2, int damageArea = 0, int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            var entPos = relativePos ?? Transform(ent).Coordinates;
            var vspread = new Vector2(_random.Next(-spread, spread), _random.Next(-spread, spread));
            var desiredPos = entPos.Position + vspread;

            Blink(ent, new(ent, desiredPos), damageArea);

            Timer.Delay((int) GetDelay(ent, BaseActionDelay));
        }
    }

    public void Blink(Entity<HierophantBossComponent> ent, EntityCoordinates whereTo, int damageArea = 0)
    {
        _audio.PlayPvs(new SoundPathSpecifier("Audio/Magic/blink.ogg"), Transform(ent).Coordinates, AudioParams.Default);

        if (damageArea > 0)
            DamageArea(ent, damageArea);

        _xform.SetCoordinates(ent, whereTo);

        // spawn in both places
        if (damageArea > 0)
            DamageArea(ent, damageArea);
    }
    #endregion

    #region Helper methods
    public EntityUid? PickTarget(Entity<HierophantBossComponent> ent)
    {
        if (!ent.Comp.Aggressive
        || !TryComp<AggressiveComponent>(ent, out var aggressive)
        || aggressive.Aggressors.Count == 0)
            return null;

        return _random.Pick<EntityUid>(aggressive.Aggressors);
    }

    public float GetDelay(Entity<HierophantBossComponent> ent, float baseDelay)
    {
        var minDelay = Math.Max(baseDelay / 5f, .1f);

        return Math.Max(baseDelay - (baseDelay * CalculateAnger(ent)), minDelay);
    }
    public float CalculateAnger(Entity<HierophantBossComponent> ent)
    {
        // calculate anger threshold based on:
        // the amount of aggressors. max amount of aggressors being 3.
        // health lost.
        // resulting in a total of 2 maximum.

        float aggroFactor = 0f, healthFactor = 0f;
        if (TryComp<AggressiveComponent>(ent, out var aggressive) && aggressive.Aggressors.Count > 0)
            aggroFactor = Normalize(aggressive.Aggressors.Count, 0f, 3f);

        if (TryComp<DestructibleComponent>(ent, out var destructible)
        && TryComp<DamageableComponent>(ent, out var damageable))
        {
            // jesus christ someone kill me.
            var maxThreshold = 100f;
            foreach (var threshold in destructible.Thresholds)
            {
                if (threshold.Trigger != null && threshold.Trigger.GetType() != typeof(DamageTrigger))
                    continue;

                var trigger = (DamageTrigger) threshold.Trigger!;

                if (trigger.Damage > maxThreshold)
                    maxThreshold = trigger.Damage;
            }

            healthFactor = Normalize((float) damageable.TotalDamage, 0f, (float) maxThreshold);
        }

        ent.Comp.Anger = Normalize(aggroFactor + healthFactor, 0, 2);

        return ent.Comp.Anger;
    }

    private float Normalize(float cur, float min, float max)
        => (cur - min) / (max - min);
    #endregion
}
