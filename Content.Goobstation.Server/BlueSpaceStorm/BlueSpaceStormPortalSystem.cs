// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Goobstation.Common.BluespaceStorm;
using Content.Server.Chat.Systems;
using Content.Shared.Maps;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.StepTrigger.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Prototypes;
using Content.Shared.Throwing;
using Content.Server.GameTicking;
using Content.Shared.GameTicking.Components;
using System.Numerics;
using System.Linq;
using Robust.Server.GameObjects;
using Content.Shared.EntityEffects;

namespace Content.Goobstation.Server.BluespaceStorm;

public sealed partial class BluespaceStormSystem : EntitySystem
{
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private StepTriggerSystem _step = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private ITileDefinitionManager _tiledef = default!;
    [Dependency] private TileSystem _tile = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private GameTicker _gameTicker = default!;
    [Dependency] private SharedEntityEffectsSystem _effects = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BluespaceStormPortalComponent, PortalPulseEvent>(OnPortalPulse);
        SubscribeLocalEvent<BluespaceStormPortalComponent, ComponentStartup>(OnInit);
        SubscribeLocalEvent<BluespaceStormPortalComponent, PortalMobsAllDeathEvent>(OnMobDeath);
        SubscribeLocalEvent<BluespaceStormPortalComponent, PortalMobSpawnEvent>(OnMobSpawn);

    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<BluespaceStormPortalComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (Paused(uid))
                continue;

            if (component.MobsSpawned && !component.MobsAllDeadEventRaised)
            {
                component.SpawnedMobs.RemoveAll(mob =>
                    TerminatingOrDeleted(mob)
                    || !TryComp<MobStateComponent>(mob, out var mobState)
                    || mobState.CurrentState == MobState.Dead);
                if (component.SpawnedMobs.Count == 0)
                {
                    component.MobsAllDeadEventRaised = true;
                    RaiseLocalEvent(uid, new PortalMobsAllDeathEvent());
                    continue;
                }
            }

            if (component.NextMobSpawnTime != TimeSpan.Zero && now >= component.NextMobSpawnTime)
            {
                component.NextMobSpawnTime = TimeSpan.Zero;
                RaiseLocalEvent(uid, new PortalMobSpawnEvent());
            }

            if (now >= component.NextPulseTime)
            {
                component.NextPulseTime = now + TimeSpan.FromSeconds(component.TimeForPulse);
                RaiseLocalEvent(uid, new PortalPulseEvent());
            }
        }
    }

    private void OnInit(Entity<BluespaceStormPortalComponent> portalEnt, ref ComponentStartup args)
    {
        var component = portalEnt.Comp;
        List<EntProtoId> mobsToSpawn = [];
        for (var i = 0; i < component.MobSpawnCount && component.MobSpawnPool.Count > 0; i++)
        {
            mobsToSpawn.Add(_random.Pick(component.MobSpawnPool));
        }
        component.MobsToSpawn = mobsToSpawn;
        component.NextMobSpawnTime = _timing.CurTime + TimeSpan.FromSeconds(component.TimeForSpawn);
        component.NextPulseTime = _timing.CurTime + TimeSpan.FromSeconds(component.TimeForPulse);
    }

    private void OnPortalPulse(Entity<BluespaceStormPortalComponent> portalEnt, ref PortalPulseEvent args)
    {
        var component = portalEnt.Comp;
        var totalWeight = 0;
        foreach (var option in component.PulseEffects)
            totalWeight += option.Weight;

        if (totalWeight <= 0)
            return;

        var roll = _random.Next(0, totalWeight);
        foreach (var option in component.PulseEffects)
        {
            if (roll < option.Weight)
            {
                _effects.TryApplyEffect(portalEnt, new ProtoId<EntityEffectPrototype>(option.Effect));
                return;
            }

            roll -= option.Weight;
        }
    }

    private void OnMobSpawn(Entity<BluespaceStormPortalComponent> portalEnt, ref PortalMobSpawnEvent args)
    {
        var uid = portalEnt.Owner;
        var component = portalEnt.Comp;
        component.MobsSpawned = true;
        foreach (EntProtoId proto in component.MobsToSpawn)
        {
            var newSpawn = Spawn(proto, _transform.GetMapCoordinates(uid));
            if (!TryComp<BluespaceStormPortalMobComponent>(newSpawn, out _))
            {
                continue;
            }
            component.SpawnedMobs.Add(newSpawn);
            Comp<BluespaceStormPortalMobComponent>(newSpawn).LinkedPortal = uid;
        }
    }

    private void OnMobDeath(Entity<BluespaceStormPortalComponent> portalEnt, ref PortalMobsAllDeathEvent args)
    {
        var query = EntityQueryEnumerator<BluespaceRuleComponent, ActiveGameRuleComponent, GameRuleComponent>();
        EntityUid ruleUID;
        while (query.MoveNext(out ruleUID, out var rule, out _, out _))
        {
            rule.PortalsRemaining = Math.Max(0, rule.PortalsRemaining - 1);
            if(rule.PortalsRemaining < 1)
            {
                _chat.DispatchGlobalAnnouncement(
                "The spacetime phenomena has subsided, please return to your stations.",
                colorOverride: Color.Green,
                playSound: false);
                _gameTicker.EndGameRule(ruleUID);
            }
            break;
        }
        EntityManager.DeleteEntity(portalEnt.Owner);
    }

    public List<TileRef>? GetSpawningPoints(EntityUid uid, float severity, float powerModifier = 1f)
    {
        var xform = Transform(uid);

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return null;


        var amount = (int) (MathHelper.Lerp(2, 5, severity * powerModifier) + 0.5f);

        var worldPos = _transform.GetWorldPosition(uid);

        var tilerefs = _map.GetTilesIntersecting(
                xform.GridUid.Value,
                grid,
                new Box2(worldPos + new Vector2(-10), worldPos + new Vector2(10))).ToList();

        if (tilerefs.Count == 0)
            return null;

        var resultList = new List<TileRef>();
        while (resultList.Count < amount)
        {
            if (tilerefs.Count == 0)
                break;

            var tileref = _random.Pick(tilerefs);

            var tileWorldPos = _map.GridTileToWorldPos(xform.GridUid.Value, grid, tileref.GridIndices);
            var distance = Vector2.Distance(tileWorldPos, worldPos);

            if (distance > 10 || distance < 1)
            {
                tilerefs.Remove(tileref);
                continue;
            }


            resultList.Add(tileref);
        }
        return resultList;
    }

}
