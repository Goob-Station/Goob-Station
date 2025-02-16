using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Linq;
using System.Numerics;
using Content.Server._Lavaland.Mobs.Hierophant.Components;
using Content.Shared._Lavaland.Aggression;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs;
using Robust.Shared.Timing;

// ReSharper disable AccessToModifiedClosure
// ReSharper disable BadListLineBreaks

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace Content.Server._Lavaland.Mobs.Hierophant;

public sealed class HierophantSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MegafaunaSystem _megafauna = default!;
    [Dependency] private readonly HierophantFieldSystem _hierophantField = default!;

    private readonly EntProtoId _damageBoxPrototype = "LavalandHierophantSquare";
    private readonly EntProtoId _chaserPrototype = "LavalandHierophantChaser";

    // Im too lazy to deal with MobThreshholds.
    private readonly FixedPoint2 _hierophantHp = 2500;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HierophantBossComponent, AttackedEvent>(OnAttacked);
        SubscribeLocalEvent<HierophantBossComponent, MobStateChangedEvent>(_megafauna.OnDeath);

        SubscribeLocalEvent<HierophantBossComponent, MegafaunaStartupEvent>(OnHierophantInit);
        SubscribeLocalEvent<HierophantBossComponent, MegafaunaDeinitEvent>(OnHierophantDeinit);
        SubscribeLocalEvent<HierophantBossComponent, MegafaunaKilledEvent>(OnHierophantKilled);
    }


    private void OnHierophantInit(Entity<HierophantBossComponent> ent, ref MegafaunaStartupEvent args)
    {
        if (ent.Comp.ConnectedFieldGenerator != null &&
            TryComp<HierophantFieldGeneratorComponent>(ent.Comp.ConnectedFieldGenerator.Value, out var fieldComp))
            _hierophantField.ActivateField((ent.Comp.ConnectedFieldGenerator.Value, fieldComp));
    }

    private void OnHierophantDeinit(Entity<HierophantBossComponent> ent, ref MegafaunaDeinitEvent args)
    {
        if (ent.Comp.ConnectedFieldGenerator == null ||
            !TryComp<HierophantFieldGeneratorComponent>(ent.Comp.ConnectedFieldGenerator.Value, out var fieldComp))
            return;

        var field = ent.Comp.ConnectedFieldGenerator.Value;
        _hierophantField.DeactivateField((field, fieldComp));

        // After 10 seconds, hierophant teleports back to it's original place
        var position = _xform.GetMapCoordinates(field);
        Timer.Spawn(TimeSpan.FromSeconds(10), () => _xform.SetMapCoordinates(ent, position));
    }

    private void OnHierophantKilled(Entity<HierophantBossComponent> ent, ref MegafaunaKilledEvent args)
    {
        if (ent.Comp.ConnectedFieldGenerator != null &&
            TryComp<HierophantFieldGeneratorComponent>(ent.Comp.ConnectedFieldGenerator.Value, out var fieldComp))
            _hierophantField.DeactivateField((ent.Comp.ConnectedFieldGenerator.Value, fieldComp));
    }

    private void OnAttacked(Entity<HierophantBossComponent> ent, ref AttackedEvent args)
    {
        _megafauna.OnAttacked(ent, ent.Comp, ref args); // invoke base

        if (!ent.Comp.Meleed)
        {
            ent.Comp.Meleed = true;
            SpawnChasers(ent);
        }

        AdjustAnger(ent, .1f);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = EntityQueryEnumerator<HierophantBossComponent, DamageableComponent>();
        while (eqe.MoveNext(out var uid, out var comp, out var damage))
        {
            Entity<HierophantBossComponent> ent = (uid, comp);

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
                    comp.AttackTimer = Math.Max(comp.AttackCooldown / comp.CurrentAnger * 0.8f, comp.MinAttackCooldown);
                    AdjustAnger(ent, -.15f);

                    comp.MajorAttackTimer += HierophantBossComponent.TileDamageDelay;
                });

                TickTimer(ent, ref comp.MajorAttackTimer, frameTime, () =>
                {
                    DoMajorAttack(ent);
                    comp.MajorAttackTimer = Math.Max(comp.MajorAttackCooldown / comp.CurrentAnger * 0.8f, comp.MinMajorAttackCooldown);
                    AdjustAnger(ent, -.25f);

                    comp.AttackTimer += HierophantBossComponent.TileDamageDelay;
                });

                if (comp.Meleed)
                {
                    TickTimer(ent, ref comp.MeleeReactionTimer, frameTime, () =>
                    {
                        comp.Meleed = false; // reset it. check OnAttack event
                        comp.MeleeReactionTimer = comp.MeleeReactionCooldown;
                    });
                }

                var newMinAnger = Math.Max((float)(damage.TotalDamage / _hierophantHp) * 3, 0f) + 1f;
                ent.Comp.MinAnger = newMinAnger;
                AdjustAnger(ent, 0); // Update anger
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
        RaiseLocalEvent(ent, new MegafaunaStartupEvent());
    }

    private void DeinitBoss(Entity<HierophantBossComponent> ent)
    {
        ent.Comp.Aggressive = false;
        ent.Comp.CancelToken.Cancel(); // cancel all stuff

        RaiseLocalEvent(ent, new MegafaunaDeinitEvent());
    }

    public async void DoMajorAttack(Entity<HierophantBossComponent> ent)
    {
        var target = PickTarget(ent);

        // Major attacks are always strong
        var rounding = MidpointRounding.AwayFromZero;

        // Attack amount is just rounded up anger
        var attackPower = (int) Math.Round(ent.Comp.CurrentAnger, rounding);
        var actions = new List<Action>
        {
            () => { DamageArea(ent, target, attackPower * 2); },
            () => { SpawnCrosses(ent, target, attackPower); },
        };

        // Add blink action if there's a target to teleport
        if (target != null)
            actions.Add( () => { BlinkRandom(ent, _xform.GetWorldPosition(target.Value), attackPower); } );

        if (TerminatingOrDeleted(ent))
            return;

        _random.Pick(actions).Invoke();
    }

    public async void DoMinorAttack(Entity<HierophantBossComponent> ent)
    {
        var target = PickTarget(ent);

        // How we round up our anger level, to bigger value or the lower.
        var rounding = _random.Next(0, 1) == 1 ? MidpointRounding.AwayFromZero : MidpointRounding.ToZero;

        // Attack amount is just rounded up anger
        var attackPower = (int) Math.Round(ent.Comp.CurrentAnger, rounding);

        var actions = new List<Action>()
        {
            () => { SpawnChasers(ent); },
            () => { SpawnCrosses(ent, target, attackPower); },
            () => { DamageArea(ent, target, attackPower + 1); },
        };

        if (TerminatingOrDeleted(ent))
            return;

        _random.Pick(actions).Invoke();
    }

    #region Patterns

    private void DamageArea(Entity<HierophantBossComponent> ent, EntityUid? target = null, int range = 1)
    {
        if (TerminatingOrDeleted(ent))
            return;

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Machines/airlock_ext_open.ogg"), ent, AudioParams.Default.WithMaxDistance(10f));

        target = (target ?? PickTarget(ent)) ?? ent;

        // we need this beacon in order for damage box to not break apart
        var beacon = Spawn(null, _xform.GetMapCoordinates((EntityUid) target));

        for (var i = 0; i <= range; i++)
        {
            if (TerminatingOrDeleted(ent))
                return;

            var delay = (int) GetDelay(ent, ent.Comp.InterActionDelay / 2.5f) * i;
            Timer.Spawn(delay,
                () =>
                {
                    SpawnDamageBox(beacon, range: i);
                });
        }

        EntityManager.DeleteEntity(beacon); // cleanup
    }

    private void SpawnChasers(Entity<HierophantBossComponent> ent, int amount = 1)
    {
        for (var i = 0; i < amount; i++)
        {
            if (TerminatingOrDeleted(ent))
                return;

            var delay = (int) GetDelay(ent, ent.Comp.InterActionDelay) * i;
            Timer.Spawn(delay,
                () =>
                {
                    var chaser = Spawn(_chaserPrototype, Transform(ent).Coordinates);
                    if (TryComp<HierophantChaserComponent>(chaser, out var chasercomp))
                    {
                        chasercomp.Target = PickTarget(ent);
                        chasercomp.MaxSteps *= ent.Comp.CurrentAnger;
                        chasercomp.Speed += ent.Comp.CurrentAnger * 0.5f;
                    }
                });
        }
    }

    private void BlinkRandom(Entity<HierophantBossComponent> ent, Vector2? relativePos, int amount = 1)
    {
        for (var i = 0; i < amount; i++)
        {
            if (TerminatingOrDeleted(ent))
                return;

            if (relativePos == null)
                return;

            var delay = (int) GetDelay(ent, ent.Comp.InterActionDelay) * i;
            Timer.Spawn(delay,
                () =>
                {
                    Blink(ent, relativePos.Value);
                });
        }
    }

    private void SpawnCrosses(Entity<HierophantBossComponent> ent, EntityUid? target, int amount = 1)
    {
        for (var i = 0; i < amount; i++)
        {
            if (TerminatingOrDeleted(ent))
                return;

            var delay = (int) GetDelay(ent, ent.Comp.InterActionDelay) * i;
            Timer.Spawn(delay,
                () =>
                {
                    target ??= ent;
                    SpawnCross(target.Value);
                });
        }
    }

    #endregion

    #region Attacks

    public void SpawnDamageBox(EntityUid relative, int range = 0, bool hollow = true)
    {
        if (range == 0)
        {
            Spawn(_damageBoxPrototype, Transform(relative).Coordinates);
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
            Spawn(_damageBoxPrototype, _map.GridTileToWorld((EntityUid) xform.GridUid, grid, tile.GridIndices));
        }
    }

    private void Blink(EntityUid ent, Vector2 worldPos)
    {
        if (TerminatingOrDeleted(ent))
            return;

        var dummy = Spawn(null, new MapCoordinates(worldPos, Transform(ent).MapID));

        SpawnDamageBox(ent, 1, false);
        SpawnDamageBox(dummy, 1, false);

        Timer.Spawn((int)(HierophantBossComponent.TileDamageDelay * 1000),
            () =>
            {
                _audio.PlayPvs(new SoundPathSpecifier("/Audio/Magic/blink.ogg"), Transform(ent).Coordinates, AudioParams.Default.WithMaxDistance(10f));
                _xform.SetWorldPosition(ent, worldPos);
                QueueDel(dummy);
            });
    }

    public void Blink(EntityUid ent, EntityUid? marker = null)
    {
        if (marker == null)
            return;

        Blink(ent, _xform.GetWorldPosition(marker.Value));
        QueueDel(marker);
    }

    public void SpawnCross(EntityUid target, float range = 10, float bothChance = 0.1f)
    {
        var xform = Transform(target);

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid) ||
            !_xform.TryGetGridTilePosition(target, out var tilePos))
            return;

        var cross = MakeCross(tilePos, range);
        var diagcross = MakeCrossDiagonal(tilePos, range);

        var types = new List<List<Vector2i>?> { cross, diagcross };
        var both = new List<Vector2i>();
        both.AddRange(cross);
        both.AddRange(diagcross);

        var tiles = _random.Prob(bothChance) ? both : _random.Pick(types);

        foreach (var tile in tiles!)
        {
            Spawn(_damageBoxPrototype, _map.GridTileToWorld((EntityUid) xform.GridUid, grid, tile));
        }
    }
    #endregion

    #region Helper methods

    private EntityUid? PickTarget(Entity<HierophantBossComponent> ent)
    {
        if (!ent.Comp.Aggressive
        || !TryComp<AggressiveComponent>(ent, out var aggressive)
        || aggressive.Aggressors.Count == 0
        || TerminatingOrDeleted(ent))
            return null;

        return _random.Pick(aggressive.Aggressors);
    }

    private float GetDelay(Entity<HierophantBossComponent> ent, float baseDelay)
    {
        var minDelay = Math.Max(baseDelay / 2.5f, HierophantBossComponent.TileDamageDelay);

        return Math.Max(baseDelay - (baseDelay * ent.Comp.CurrentAnger), minDelay);
    }

    private void AdjustAnger(Entity<HierophantBossComponent> ent, float anger)
    {
        ent.Comp.CurrentAnger = Math.Clamp(ent.Comp.CurrentAnger + anger, 0, ent.Comp.MaxAnger);
        if (ent.Comp.CurrentAnger < ent.Comp.MinAnger)
            ent.Comp.CurrentAnger = ent.Comp.MinAnger;
    }

    private List<Vector2i> MakeCross(Vector2i tilePos, float range)
    {
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
    private List<Vector2i> MakeCrossDiagonal(Vector2i tilePos, float range)
    {
        var refs = new List<Vector2i>();
        var center = tilePos;

        refs.Add(center);

        // we go thru all directions and fill the array up
        for (var i = 1; i < range; i++)
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
