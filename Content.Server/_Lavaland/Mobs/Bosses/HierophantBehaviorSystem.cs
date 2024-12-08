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
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

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

        SubscribeLocalEvent<HierophantBossComponent, AttackedEvent>(OnAttacked);
        SubscribeLocalEvent<HierophantBossComponent, DamageThresholdReached>(_megafauna.OnDeath);
    }

    private void OnAttacked(Entity<HierophantBossComponent> ent, ref AttackedEvent args)
    {
        _megafauna.OnAttacked(ent, ent.Comp, ref args); // invoke base

        if (!ent.Comp.Meleed)
        {
            ent.Comp.Meleed = true;
            DamageArea(ent, ent, 5);
            SpawnChasers(ent, 2);
        }

        AdjustAnger(ent, .1f);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = EntityQueryEnumerator<HierophantBossComponent>();
        while (eqe.MoveNext(out var uid, out var comp))
        {
            var ent = (uid, comp);

            if (TryComp<AggressiveComponent>(uid, out var aggressors))
            {
                if (aggressors.Aggressors.Count > 0 && !comp.Aggressive)
                    InitBoss(ent);
                else if (aggressors.Aggressors.Count == 0 && comp.Aggressive)
                    DeinitBoss(ent);
            }

            if (comp.Aggressive)
            {
                // tick all timers

                TickTimer(ent, ref comp.AttackTimer, frameTime, () =>
                {
                    DoMinorAttack(ent);
                    comp.AttackTimer = Math.Max(comp.AttackCooldown - comp.Anger, 1f);
                    AdjustAnger(ent, -.15f);
                });

                TickTimer(ent, ref comp.MajorAttackTimer, frameTime, () =>
                {
                    DoMajorAttack(ent);
                    comp.MajorAttackTimer = Math.Max(comp.MajorAttackCooldown - comp.Anger, 2f);
                    AdjustAnger(ent, -.25f);
                });

                if (comp.Meleed)
                {
                    TickTimer(ent, ref comp.MeleeReactionTimer, frameTime, () =>
                    {
                        comp.Meleed = false; // reset it. check OnAttack event
                        comp.MeleeReactionTimer = comp.MeleeReactionCooldown;
                    });
                }
            }
        }
    }

    private void TickTimer(Entity<HierophantBossComponent> ent, ref float timer, float frameTime, Action onFired)
    {
        timer -= frameTime;

        if (timer <= 0 && !ent.Comp.IsAttacking) // check if he is currently attacking just in case
        {
            ent.Comp.IsAttacking = true;

            onFired.Invoke();

            ent.Comp.IsAttacking = false;
        }
    }

    private void InitBoss(Entity<HierophantBossComponent> ent)
    {
        ent.Comp.Aggressive = true;

        // add ambient
        if (TryComp<AmbientSoundComponent>(ent, out var ambient))
            _ambient.SetAmbience(ent, true, ambient);
    }
    private void DeinitBoss(Entity<HierophantBossComponent> ent)
    {
        ent.Comp.Aggressive = false;

        if (TryComp<AmbientSoundComponent>(ent, out var ambient))
            _ambient.SetAmbience(ent, false, ambient);

        ent.Comp.CancelToken.Cancel(); // cancel all stuff
    }

    public async void DoMajorAttack(Entity<HierophantBossComponent> ent)
    {
        var target = PickTarget(ent);
        if (target == null)
            target = ent;

        var attackPower = _random.Next(3, 5);
        var attackPowerAngered = (int) (attackPower + ent.Comp.Anger);

        var actions = new List<Action>()
        {
            //() => { BlinkRandom(ent, _xform.GetWorldPosition((EntityUid) target), (int) (attackPower / 2)); },
            () => { DamageArea(ent, target, attackPowerAngered); },
            () => { SpawnCrosses(ent, target, attackPowerAngered); }
            // todo spawn crosses
        };

        _random.Pick(actions).Invoke();
    }
    public async void DoMinorAttack(Entity<HierophantBossComponent> ent)
    {
        var target = PickTarget(ent);
        if (target == null)
            target = ent;

        var attackPower = _random.Next(1, 3);

        var actions = new List<Action>()
        {
            () => { BlinkRandom(ent, _xform.GetWorldPosition((EntityUid) target)); },
            () => { SpawnChasers(ent, attackPower); },
            () => { SpawnCrosses(ent, target, attackPower); }
            // todo spawn crosses
        };

        _random.Pick(actions).Invoke();
    }

    #region Patterns
    public async Task DamageArea(Entity<HierophantBossComponent> ent, EntityUid? target = null, int range = 1)
    {
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Machines/airlock_ext_open.ogg"), ent, AudioParams.Default.WithMaxDistance(10f));

        target = target ?? PickTarget(ent);
        if (target == null)
            target = ent;

        // we need this beacon in order for damage box to not break apart
        var beacon = Spawn(null, _xform.GetMapCoordinates((EntityUid) target));

        for (int i = 0; i <= range; i++)
        {
            SpawnDamageBox(beacon, range: i);
            await Task.Delay((int) GetDelay(ent, BaseActionDelay / 2.5f));
        }

        EntityManager.DeleteEntity(beacon); // cleanup
    }

    public async Task SpawnChasers(Entity<HierophantBossComponent> ent, int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            var chaser = Spawn(ChaserPrototype, Transform(ent).Coordinates);
            if (TryComp<HierophantChaserComponent>(chaser, out var chasercomp))
            {
                chasercomp.Target = PickTarget(ent);
                chasercomp.Speed *= ent.Comp.Anger;
                chasercomp.MaxSteps *= ent.Comp.Anger;
            }

            await Task.Delay(1000);
        }
    }

    public async Task BlinkRandom(Entity<HierophantBossComponent> ent, Vector2? relativePos, int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            var entPos = relativePos ?? _xform.GetWorldPosition((EntityUid) ent);
            await Blink(ent, entPos);
            await Task.Delay((int) GetDelay(ent, BaseActionDelay));
        }
    }

    public async Task SpawnCrosses(Entity<HierophantBossComponent> ent, EntityUid? target, int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            target = target ?? ent;
            SpawnCross(ent, (EntityUid) target);
            await Task.Delay((int) GetDelay(ent, BaseActionDelay));
        }
    }
    #endregion

    #region Attacks
    public void SpawnDamageBox(EntityUid relative, int range = 0, bool hollow = true)
    {
        if (range == 0)
        {
            Spawn(DamageBoxPrototype, Transform(relative).Coordinates);
            return;
        }

        var xform = Transform(relative);

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return;

        var gridEnt = ((EntityUid) xform.GridUid, grid);

        // get tile position of our entity
        if (!_xform.TryGetGridTilePosition(relative, out var tilePos))
            return;

        // make a box
        var pos = _map.TileCenterToVector(gridEnt, tilePos);
        var confines = new Box2(pos, pos).Enlarged(range);
        var box = _map.GetLocalTilesIntersecting(relative, grid, confines).ToList();

        // hollow it out if necessary
        if (hollow)
        {
            var confinesS = new Box2(pos, pos).Enlarged(Math.Max(range - 1, 0));
            var boxS = _map.GetLocalTilesIntersecting(relative, grid, confinesS).ToList();
            box = box.Where(b => !boxS.Contains(b)).ToList();
        }

        // fill the box
        foreach (var tile in box)
        {
            Spawn(DamageBoxPrototype, _map.GridTileToWorld((EntityUid) xform.GridUid, grid, tile.GridIndices));
        }
    }
    public async Task Blink(Entity<HierophantBossComponent> ent, Vector2 worldPos)
    {
        var dummy = Spawn(null, new MapCoordinates(worldPos, Transform(ent).MapID));

        SpawnDamageBox(ent, 1, false);
        SpawnDamageBox(dummy, 1, false);

        await Task.Delay(600); // 600ms according to the chargeup of hiero damage square

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Magic/blink.ogg"), Transform(ent).Coordinates, AudioParams.Default.WithMaxDistance(10f));

        _xform.SetWorldPosition(ent, worldPos);
        EntityManager.DeleteEntity(dummy);
    }
    public void SpawnCross(Entity<HierophantBossComponent> ent, EntityUid target, float range = 10)
    {
        var xform = Transform(target);

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return;

        var cross = MakeCross(target, range);
        var diagcross = MakeCrossDiagonal(target, range);

        if (cross == null || diagcross == null)
            return;

        var types = new List<List<Vector2i>?>() { cross, diagcross };
        var both = new List<Vector2i>();
        both.AddRange(cross);
        both.AddRange(diagcross);


        var tiles = new List<Vector2i>();
        if (_random.Prob(.1f)) // 10%
            tiles = both;
        else tiles = _random.Pick(types);

        foreach (var tile in tiles!)
            Spawn(DamageBoxPrototype, _map.GridTileToWorld((EntityUid) xform.GridUid, grid, tile));
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
        var minDelay = Math.Max(baseDelay / 2.5f, .1f);

        return Math.Max(baseDelay - (baseDelay * ent.Comp.Anger), minDelay);
    }
    public void AdjustAnger(Entity<HierophantBossComponent> ent, float anger)
    {
        ent.Comp.Anger = Math.Clamp(ent.Comp.Anger + anger, 0, 3);
    }

    private float Normalize(float cur, float min, float max)
        => Math.Clamp((cur - min) / (max - min), min, max);

    private List<Vector2i>? MakeCross(EntityUid relative, float range)
    {
        var xform = Transform(relative);

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return null;

        var gridEnt = ((EntityUid) xform.GridUid, grid);

        // get tile position of our entity
        if (!_xform.TryGetGridTilePosition(relative, out var tilePos))
            return null;

        var refs = new List<Vector2i>();
        var center = tilePos;

        refs.Add(center);

        // we go thru all directions and fill the array up
        for (int i = 1; i < range; i++)
        {
            // this should make a neat cross
            refs.Add(new Vector2i(center.X + i, center.Y));
            refs.Add(new Vector2i(center.X, center.Y + i));
            refs.Add(new Vector2i(center.X - i, center.Y));
            refs.Add(new Vector2i(center.X, center.Y - i));
        }

        return refs;
    }
    private List<Vector2i>? MakeCrossDiagonal(EntityUid relative, float range)
    {
        var xform = Transform(relative);

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return null;

        var gridEnt = ((EntityUid) xform.GridUid, grid);

        // get tile position of our entity
        if (!_xform.TryGetGridTilePosition(relative, out var tilePos))
            return null;

        var refs = new List<Vector2i>();
        var center = tilePos;

        refs.Add(center);

        // we go thru all directions and fill the array up
        for (int i = 1; i < range; i++)
        {
            // this should make a neat diagonal cross
            refs.Add(new Vector2i(center.X + i, center.Y + i));
            refs.Add(new Vector2i(center.X + i, center.Y - i));
            refs.Add(new Vector2i(center.X - i, center.Y + i));
            refs.Add(new Vector2i(center.X - i, center.Y - i));
        }

        return refs;
    }
    #endregion
}
