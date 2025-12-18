using System.Linq;
using System.Numerics;
using Content.Goobstation.Common.Physics;
using Content.Server.Chat.Systems;
using Content.Server.Ghost;
using Content.Server.Ghost.Roles;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Popups;
using Content.Shared._Goobstation.Wizard.FadingTimedDespawn;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Systems;
using Content.Shared._Shitmed.Damage;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Administration.Logs;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.Heretic;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Robust.Server.Audio;
using Robust.Server.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server._Shitcode.Heretic.EntitySystems.PathSpecific;

public sealed class StarGazerSystem : SharedStarGazerSystem
{
    [Dependency] private readonly PvsOverrideSystem _pvs = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly DamageableSystem _dmg = default!;
    [Dependency] private readonly SharedStarMarkSystem _mark = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly ThrowingSystem _throw = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly GhostRoleSystem _ghostRole = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;

    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ISharedAdminLogManager _admin = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LaserBeamEndpointComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<LaserBeamEndpointComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<CosmosPassiveComponent, ResetStarGazerConsciousnessEvent>(OnReset);
        SubscribeLocalEvent<CosmosPassiveComponent, ComponentShutdown>(OnPassiveShutdown);
        SubscribeLocalEvent<CosmosPassiveComponent, MobStateChangedEvent>(OnMobStateChanged);

        SubscribeLocalEvent<StarGazerComponent, StarGazerSeekMasterEvent>(OnSeekMaster);
        SubscribeLocalEvent<StarGazerComponent, TakeGhostRoleEvent>(OnTakeGhostRole,
            after: [typeof(GhostRoleSystem)]);

        SubscribeLocalEvent<GhostAttemptHandleEvent>(OnGhost);
    }

    private void OnGhost(GhostAttemptHandleEvent args)
    {
        if (HasComp<StarGazerComponent>(args.Mind.CurrentEntity))
            args.CanReturnGlobal = false;
    }

    private void OnSeekMaster(Entity<StarGazerComponent> ent, ref StarGazerSeekMasterEvent args)
    {
        if (!Exists(ent.Comp.Summoner))
            return;

        args.Handled = true;

        TeleportStarGazer(ent, ent.Comp.Summoner);
    }

    private void TeleportStarGazer(Entity<StarGazerComponent> ent, EntityUid target)
    {
        var xform = Transform(ent);

        _audio.PlayPvs(ent.Comp.TeleportSound, xform.Coordinates);
        Spawn(ent.Comp.TeleportEffect, xform.Coordinates);
        _pulling.StopAllPulls(ent);
        Xform.SetMapCoordinates((ent.Owner, xform), Xform.GetMapCoordinates(target));
        Spawn(ent.Comp.TeleportEffect, xform.Coordinates);
        _audio.PlayPvs(ent.Comp.TeleportSound, xform.Coordinates);
    }

    private void OnMobStateChanged(Entity<CosmosPassiveComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
        {
            if (!Exists(ent.Comp.StarGazer) || TerminatingOrDeleted(ent.Comp.StarGazer.Value))
                return;

            KillStarGazer(ent.Comp.StarGazer.Value);
            return;
        }

        if (args.NewMobState != MobState.Alive)
            return;

        var starGazer = ResolveStarGazer((ent.Owner, null, ent.Comp), out _);
        if (starGazer == null)
            return;

        RemCompDeferred<FadingTimedDespawnComponent>(starGazer.Value);
    }

    private void OnPassiveShutdown(Entity<CosmosPassiveComponent> ent, ref ComponentShutdown args)
    {
        if (!Exists(ent.Comp.StarGazer) || TerminatingOrDeleted(ent.Comp.StarGazer.Value))
            return;

        KillStarGazer(ent.Comp.StarGazer.Value);
    }

    private void KillStarGazer(EntityUid starGazer)
    {
        var fading = EnsureComp<FadingTimedDespawnComponent>(starGazer);
        fading.FadeOutTime = 5f;
        fading.Lifetime = 0f;
    }

    private void OnTakeGhostRole(Entity<StarGazerComponent> ent, ref TakeGhostRoleEvent args)
    {
        if (!args.TookRole || ent.Comp.ResettingMindSession == null)
            return;

        _popup.PopupCoordinates(Loc.GetString("heretic-stargazer-consciousness-reset-target"),
            Transform(ent).Coordinates,
            ent.Comp.ResettingMindSession,
            PopupType.LargeCaution);

        _popup.PopupEntity(Loc.GetString("heretic-stargazer-consciousness-reset-user"),
            ent.Comp.Summoner,
            ent.Comp.Summoner,
            PopupType.Large);

        ent.Comp.ResettingMindSession = null;
    }

    private void OnReset(Entity<CosmosPassiveComponent> ent, ref ResetStarGazerConsciousnessEvent args)
    {
        args.Handled = true;

        var starGazer = ResolveStarGazer(ent.Owner, out var spawned);
        if (starGazer == null || spawned)
            return;

        if (TryComp(starGazer.Value, out ActorComponent? actor))
            starGazer.Value.Comp.ResettingMindSession = actor.PlayerSession;

        EnsureComp<GhostTakeoverAvailableComponent>(starGazer.Value).IgnoreMindCheck = true;
        var role = EnsureComp<GhostRoleComponent>(starGazer.Value);
        _ghostRole.SetTaken(role, false);
        _ghostRole.RegisterGhostRole((starGazer.Value, role));
    }

    private void RemoveGhostRole(Entity<StarGazerComponent, GhostRoleComponent?> ent, bool hasMind, bool resettingMind)
    {
        ent.Comp1.GhostRoleAccumulator = 0f;
        ent.Comp1.ResettingMindSession = null;

        if (!hasMind || resettingMind || !Resolve(ent, ref ent.Comp2, false) || ent.Comp2.Taken)
            return;

        _ghostRole.SetTaken(ent.Comp2, true);
        _ghostRole.UnregisterGhostRole((ent.Owner, ent.Comp2));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var xformQuery = GetEntityQuery<TransformComponent>();
        var jointQuery = GetEntityQuery<ComplexJointVisualsComponent>();
        var mobStateQuery = GetEntityQuery<MobStateComponent>();
        var starGazeQuery = GetEntityQuery<StarGazeComponent>();
        var ghostRoleQuery = GetEntityQuery<GhostRoleComponent>();
        var actorQuery = GetEntityQuery<ActorComponent>();

        var query = EntityQueryEnumerator<StarGazerComponent, MindContainerComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var starGazer, out var mindContainer, out var xform))
        {
            var hasMind = mindContainer.HasMind;
            var resettingMind = starGazer.ResettingMindSession != null;
            var changedSession = resettingMind && (!actorQuery.TryComp(uid, out var actor) ||
                actor.PlayerSession != starGazer.ResettingMindSession);

            if (changedSession)
                RemoveGhostRole((uid, starGazer), hasMind, resettingMind);
            else if (hasMind && resettingMind && ghostRoleQuery.TryComp(uid, out var ghostRole))
            {
                starGazer.GhostRoleAccumulator += frameTime;

                if (starGazer.GhostRoleAccumulator > starGazer.GhostRoleTimer)
                {
                    RemoveGhostRole((uid, starGazer, ghostRole), hasMind, resettingMind);
                    _popup.PopupEntity(Loc.GetString("heretic-stargazer-consciousness-reset-fail"),
                        starGazer.Summoner,
                        starGazer.Summoner,
                        PopupType.Large);
                }
            }
            else
                RemoveGhostRole((uid, starGazer), hasMind, resettingMind);

            starGazer.ResetDistanceAccumulator += frameTime;

            if (starGazer.ResetDistanceAccumulator > starGazer.ResetDistanceTimer)
            {
                starGazer.ResetDistanceAccumulator = 0f;

                if (Exists(starGazer.Summoner) &&
                    !Xform.InRange((uid, xform), starGazer.Summoner, starGazer.MaxDistance))
                    TeleportStarGazer((uid, starGazer), starGazer.Summoner);
            }

            if (!starGazeQuery.TryComp(uid, out var starGaze))
                continue;

            if (!starGaze.StartedBlasting)
                continue;

            if (!jointQuery.TryComp(uid, out var joint))
            {
                QueueDel(starGaze.Endpoint);
                RemCompDeferred(uid, starGaze);
                continue;
            }

            starGaze.TimeSinceBeamCreation += frameTime;

            var time = starGaze.TimeSinceBeamCreation;

            if (time > starGaze.Duration)
            {
                ClearJoints(uid, joint);
                QueueDel(starGaze.Endpoint);
                RemCompDeferred(uid, starGaze);
                continue;
            }

            var stage = GetBeamStage(time);
            Dictionary<NetEntity, ComplexJointVisualsData>? jointData = null;

            if (stage != starGaze.LastStage)
            {
                starGaze.LastStage = stage;

                jointData = GetJointData(joint);
                foreach (var data in jointData.Values)
                {
                    if (data.Id != JointId)
                        continue;

                    var startSprite = starGaze.Start2;
                    var beamSprite = starGaze.Beam2;
                    var endSprite = starGaze.End2;
                    switch (stage)
                    {
                        case 1:
                            startSprite = starGaze.Start1;
                            beamSprite = starGaze.Beam1;
                            endSprite = starGaze.End1;
                            break;
                        case 3:
                            startSprite = starGaze.Start3;
                            beamSprite = starGaze.Beam3;
                            endSprite = starGaze.End3;
                            break;
                    }

                    if (data.StartSprite == startSprite)
                        continue;

                    data.StartSprite = startSprite;
                    data.Sprite = beamSprite;
                    data.EndSprite = endSprite;
                    Dirty(uid, joint);
                }
            }

            starGaze.Accumulator += frameTime;

            if (starGaze.Accumulator < starGaze.UpdateInterval)
                continue;

            starGaze.Accumulator = 0;

            var exists = Exists(starGaze.Endpoint);
            if (!exists || starGaze.CursorPosition == null)
            {
                ClearJoints(uid, joint, jointData);

                if (exists)
                    QueueDel(starGaze.Endpoint!.Value);

                RemCompDeferred(uid, starGaze);
                continue;
            }

            var target = starGaze.CursorPosition.Value;
            var endpoint = starGaze.Endpoint!.Value;
            var endpointXform = xformQuery.GetComponent(endpoint);
            var pos = Xform.GetWorldPosition(endpointXform, xformQuery);
            var dir = target.Position - pos;
            var len = dir.Length();

            var gazerPos = Xform.GetWorldPosition(xform, xformQuery);
            var newPos = pos + dir * starGaze.LaserSpeed / len;
            var dir2 = newPos - gazerPos;
            var len2 = dir2.Length();

            if (len2 < 0.01f)
                continue;

            if (len <= starGaze.LaserSpeed)
                Xform.SetMapCoordinates((endpoint, endpointXform), target);
            else
            {
                var newLen = Math.Clamp(len2, starGaze.MinMaxLaserRange.X, starGaze.MinMaxLaserRange.Y);

                Xform.SetMapCoordinates((endpoint, endpointXform),
                    new MapCoordinates(gazerPos + dir2 * newLen / len2, xform.MapID));
            }

            starGaze.DamageAccumulator += MathF.Max(frameTime, starGaze.UpdateInterval);

            if (starGaze.DamageAccumulator < starGaze.DamageInterval || stage != 2)
                continue;

            starGaze.DamageAccumulator = 0f;

            var c = pos - gazerPos;
            var cLen = c.Length();

            if (cLen <= 0.01f)
                continue;

            var cNorm = c / cLen;
            var angle = c.ToAngle();

            var offset = cNorm * starGaze.BeamScale;
            var box = new Box2(gazerPos + offset + new Vector2(0f, -starGaze.LaserThickness),
                gazerPos + offset + new Vector2(cLen, starGaze.LaserThickness));
            var boxRot = new Box2Rotated(box, angle, gazerPos + offset);

            var noobs = _lookup.GetEntitiesIntersecting(xform.MapID, boxRot, LookupFlags.Dynamic);
            foreach (var noob in noobs)
            {
                if (noob == starGazer.Summoner)
                    continue;

                if (!mobStateQuery.TryComp(noob, out var mobState))
                    continue;

                if (_mobState.IsIncapacitated(noob, mobState))
                {
                    var coords = xformQuery.Comp(noob).Coordinates;
                    _admin.Add(LogType.Gib,
                        LogImpact.Medium,
                        $"{ToPrettyString(uid):user} ashed {ToPrettyString(noob):target} using star gazer laser beam");
                    /* Annoying popup spam
                    _popup.PopupCoordinates(Loc.GetString("heretic-stargaze-obliterate-other",
                            ("uid", Identity.Entity(noob, EntityManager))),
                        coords,
                        Filter.PvsExcept(noob),
                        true,
                        PopupType.LargeCaution);*/
                    _popup.PopupCoordinates(Loc.GetString("heretic-stargaze-obliterate-user"),
                        coords,
                        noob,
                        PopupType.LargeCaution);
                    _audio.PlayPvs(starGaze.ObliterateSound, coords);
                    Spawn(starGaze.AshProto, coords);
                    QueueDel(noob); // Goodbye
                    continue;
                }

                _mark.TryApplyStarMark((noob, mobState));
                _dmg.TryChangeDamage(noob,
                    starGaze.Damage,
                    origin: uid,
                    targetPart: TargetBodyPart.Vital,
                    splitDamage: SplitDamageBehavior.SplitEnsureAll);

                if (_random.Prob(starGaze.ScreamProb))
                    _chat.TryEmoteWithChat(noob, "Scream");
            }

            var boxRot2 = new Box2Rotated(box.Enlarged(starGaze.GravityPullSizeModifier), angle, gazerPos + offset);
            var noobs2 = _lookup.GetEntitiesIntersecting(xform.MapID, boxRot2, LookupFlags.Dynamic);
            foreach (var noob in noobs2)
            {
                if (noob == starGazer.Summoner)
                    continue;

                if (!mobStateQuery.HasComp(noob))
                    continue;

                var noobXform = xformQuery.Comp(noob);
                var noobPos = Xform.GetWorldPosition(noobXform, xformQuery);

                var a = pos + offset - noobPos;
                var b = gazerPos + offset - noobPos;
                var aLen = a.Length();
                var bLen = b.Length();

                if (aLen <= 0.01f || bLen <= 0.01f)
                    continue;

                var angleac = Robust.Shared.Maths.Vector3.CalculateAngle(new Robust.Shared.Maths.Vector3(-a),
                    new Robust.Shared.Maths.Vector3(-c));
                var anglebc = Robust.Shared.Maths.Vector3.CalculateAngle(new Robust.Shared.Maths.Vector3(-b),
                    new Robust.Shared.Maths.Vector3(c));

                var sinac = MathF.Sin(angleac);
                var sinbc = MathF.Sin(anglebc);
                var anothersin = MathF.Sin(angleac + anglebc);
                var dist = cLen * sinac * sinbc / anothersin;

                var list = new List<(Vector2, float)>([(a / aLen, aLen), (b / bLen, bLen)]);

                var try1 = Angle.FromDegrees(90).RotateVec(cNorm);
                var try1Pos = noobPos + try1 * dist * 2f;
                var try2 = -try1;
                var try2Pos = noobPos + try2 * dist * 2f;

                if (DoIntersect(gazerPos + offset, pos + offset, noobPos, try1Pos))
                    list.Add((try1, dist));
                else if (DoIntersect(gazerPos + offset, pos + offset, noobPos, try2Pos))
                    list.Add((try2, dist));

                var result = list.MinBy(x => x.Item2);

                if (result.Item2 <= 0.01f)
                    continue;

                var throwDir = result.Item1 * MathF.Min(starGaze.MaxThrowLength, result.Item2);
                _throw.TryThrow(noob,
                    throwDir,
                    starGaze.ThrowSpeed,
                    recoil: false,
                    animated: false,
                    doSpin: false,
                    playSound: false);
            }
        }
    }

    private static Dictionary<NetEntity, ComplexJointVisualsData> GetJointData(ComplexJointVisualsComponent joint)
    {
        return joint.Data.Where(x => x.Value.Id == JointId).ToDictionary();
    }

    private void ClearJoints(EntityUid uid,
        ComplexJointVisualsComponent joint,
        Dictionary<NetEntity, ComplexJointVisualsData>? jointData = null)
    {
        jointData ??= GetJointData(joint);

        if (joint.Data.Count >= jointData.Count)
            RemCompDeferred(uid, joint);
        else
        {
            joint.Data = joint.Data.ExceptBy(jointData.Keys, kvp => kvp.Key).ToDictionary();
            Dirty(uid, joint);
        }
    }

    public static int GetOrientation(Vector2 a, Vector2 b, Vector2 c)
    {
        var val = (b.Y - a.Y) * (c.X - b.X) - (b.X - a.X) * (c.Y - b.Y);

        if (val == 0)
            return 0;

        return val > 0 ? 1 : 2;
    }

    public static bool OnSegment(Vector2 a, Vector2 b, Vector2 c)
    {
        return b.X <= Math.Max(a.X, c.X) && b.X >= Math.Min(a.X, c.X) &&
               b.Y <= Math.Max(a.Y, c.Y) && b.Y >= Math.Min(a.Y, c.Y);
    }

    public static bool DoIntersect(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2)
    {
        // Find the four orientations needed for general and special cases
        var o1 = GetOrientation(p1, q1, p2);
        var o2 = GetOrientation(p1, q1, q2);
        var o3 = GetOrientation(p2, q2, p1);
        var o4 = GetOrientation(p2, q2, q1);

        // General case: segments intersect if orientations are different
        if (o1 != o2 && o3 != o4)
            return true;

        // Special Cases (collinear points)
        // p1, q1 and p2 are collinear and p2 lies on segment p1q1
        if (o1 == 0 && OnSegment(p1, p2, q1))
            return true;

        // p1, q1 and q2 are collinear and q2 lies on segment p1q1
        if (o2 == 0 && OnSegment(p1, q2, q1))
            return true;

        // p2, q2 and p1 are collinear and p1 lies on segment p2q2
        if (o3 == 0 && OnSegment(p2, p1, q2))
            return true;

        // p2, q2 and q1 are collinear and q1 lies on segment p2q2
        if (o4 == 0 && OnSegment(p2, q1, q2))
            return true;

        return false; // Doesn't fall in any of the above cases
    }

    private static int GetBeamStage(float time)
    {
        return time < 0.8f ? 1 : time > 9.7f ? 3 : 2;
    }

    private void OnShutdown(Entity<LaserBeamEndpointComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.PvsOverride)
            _pvs.RemoveGlobalOverride(ent);
    }

    private void OnStartup(Entity<LaserBeamEndpointComponent> ent, ref ComponentStartup args)
    {
        if (ent.Comp.PvsOverride)
            _pvs.AddGlobalOverride(ent);
    }

    protected override void OnStarGazeStartup(Entity<StarGazeComponent> ent, ref ComponentStartup args)
    {
        base.OnStarGazeStartup(ent, ref args);

        _pvs.AddGlobalOverride(ent);
    }

    protected override void OnStarGazeShutdown(Entity<StarGazeComponent> ent, ref ComponentShutdown args)
    {
        base.OnStarGazeShutdown(ent, ref args);

        _pvs.RemoveGlobalOverride(ent);
    }
}
