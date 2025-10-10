using System.Linq;
using System.Numerics;
using Content.Goobstation.Common.Physics;
using Content.Server.Chat.Systems;
using Content.Server.Popups;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Systems;
using Content.Shared._Shitmed.Damage;
using Content.Shared.Damage;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
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
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LaserBeamEndpointComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<LaserBeamEndpointComponent, ComponentShutdown>(OnShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var xformQuery = GetEntityQuery<TransformComponent>();
        var jointQuery = GetEntityQuery<ComplexJointVisualsComponent>();
        var mobStateQuery = GetEntityQuery<MobStateComponent>();

        var query = EntityQueryEnumerator<StarGazeComponent, StarGazerComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var starGaze, out var starGazer, out var xform))
        {
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

            var jointData = joint.Data.Where(x => x.Value.Id == JointId).ToDictionary();

            if (time > starGaze.Duration)
            {
                ClearJoints();
                QueueDel(starGaze.Endpoint);
                RemCompDeferred(uid, starGaze);
                continue;
            }

            var stage = GetBeamStage(time);

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

            starGaze.Accumulator += frameTime;

            if (starGaze.Accumulator < starGaze.UpdateInterval)
                continue;

            starGaze.Accumulator = 0;

            var exists = Exists(starGaze.Endpoint);
            if (!exists || starGaze.CursorPosition == null)
            {
                ClearJoints();

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

            var angle = c.ToAngle();
            var box = new Box2(gazerPos + new Vector2(0f, -starGaze.LaserThickness),
                gazerPos + new Vector2(cLen, starGaze.LaserThickness));
            var boxRot = new Box2Rotated(box, angle, gazerPos);

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
                    _popup.PopupCoordinates(Loc.GetString("heretic-stargaze-obliterate-other",
                            ("uid", Identity.Entity(noob, EntityManager))),
                        coords,
                        Filter.PvsExcept(noob),
                        true,
                        PopupType.LargeCaution);
                    _popup.PopupCoordinates(Loc.GetString("heretic-stargaze-obliterate-user"),
                        coords,
                        noob,
                        PopupType.LargeCaution);
                    _audio.PlayPvs(starGaze.ObliterateSound, coords);
                    Spawn(starGaze.AshProto, coords);
                    QueueDel(noob); // Goodbye
                    continue;
                }

                _mark.TryApplyStarMark((noob, mobState), uid, true);
                _dmg.TryChangeDamage(noob,
                    starGaze.Damage,
                    origin: uid,
                    splitDamage: SplitDamageBehavior.SplitEnsureAll);

                if (_random.Prob(starGaze.ScreamProb))
                    _chat.TryEmoteWithChat(noob, "Scream");
            }

            var cNorm = c / cLen;

            var boxRot2 = new Box2Rotated(box.Enlarged(starGaze.GravityPullSizeModifier), angle, gazerPos);
            var noobs2 = _lookup.GetEntitiesIntersecting(xform.MapID, boxRot2, LookupFlags.Dynamic);
            foreach (var noob in noobs2)
            {
                if (noob == starGazer.Summoner)
                    continue;

                if (!mobStateQuery.HasComp(noob))
                    continue;

                var noobXform = xformQuery.Comp(noob);
                var noobPos = Xform.GetWorldPosition(noobXform, xformQuery);

                var a = pos - noobPos;
                var b = gazerPos - noobPos;
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

                if (DoIntersect(gazerPos, pos, noobPos, try1Pos))
                    list.Add((try1, dist));
                else if (DoIntersect(gazerPos, pos, noobPos, try2Pos))
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

            continue;

            void ClearJoints()
            {
                if (joint.Data.Count >= jointData.Count)
                    RemCompDeferred(uid, joint);
                else
                {
                    joint.Data = joint.Data.ExceptBy(jointData.Keys, kvp => kvp.Key).ToDictionary();
                    Dirty(uid, joint);
                }
            }
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

    private int GetBeamStage(float time)
    {
        return time < 0.7f ? 1 : time > 9.8f ? 3 : 2;
    }
}
