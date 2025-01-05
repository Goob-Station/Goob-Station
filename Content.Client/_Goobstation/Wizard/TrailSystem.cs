using Content.Shared._Goobstation.Wizard.Projectiles;
using Content.Shared._Goobstation.Wizard.TimeStop;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;
using Robust.Shared.Spawners;

namespace Content.Client._Goobstation.Wizard;

public sealed class TrailSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        _overlay.AddOverlay(new TrailOverlay(EntityManager, _protoMan));

        SubscribeLocalEvent<TrailComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<TrailComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<TrailComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.Accumulator = ent.Comp.Frequency;
    }

    private void OnRemove(Entity<TrailComponent> ent, ref ComponentRemove args)
    {
        var (uid, comp) = ent;

        if (comp.TrailData.Count == 0 || comp.Frequency <= 0f || comp.Lifetime <= 0f)
            return;

        var remainingTrail = Spawn(null, _transform.GetMapCoordinates(uid));
        EnsureComp<TimedDespawnComponent>(remainingTrail).Lifetime = comp.Lifetime;
        var trail = EnsureComp<TrailComponent>(remainingTrail);
        trail.Frequency = 0f;
        trail.Lifetime = comp.Lifetime;
        trail.ColorLerpAmount = comp.ColorLerpAmount;
        trail.ThicknessLerpAmount = comp.ThicknessLerpAmount;
        trail.Sprite = comp.Sprite;
        trail.Color = comp.Color;
        trail.LineThickness = comp.LineThickness;
        trail.TrailData = comp.TrailData;
        trail.Shader = comp.Shader;
        if (comp.ColorLerpAmount > 0f)
            trail.TrailData.Sort((x, y) => x.Color.A.CompareTo(y.Color.A));
        else if (comp.ThicknessLerpAmount > 0f)
            trail.TrailData.Sort((x, y) => x.Thickness.CompareTo(y.Thickness));
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlay.RemoveOverlay<TrailOverlay>();
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var xformQuery = GetEntityQuery<TransformComponent>();
        var frozenQuery = GetEntityQuery<FrozenComponent>();

        var query = EntityQueryEnumerator<TrailComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var trail, out var xform))
        {
            if (trail.Lifetime <= 0f)
                continue;

            if (frozenQuery.HasComp(uid))
                continue;

            if (trail.ColorLerpAmount > 0f || trail.ThicknessLerpAmount > 0f)
            {
                foreach (var data in trail.TrailData)
                {
                    if (trail.ColorLerpAmount > 0f)
                        data.Color = data.Color.WithAlpha(float.Lerp(data.Color.A, 0f, trail.ColorLerpAmount));
                    if (trail.ThicknessLerpAmount > 0f)
                        data.Thickness = float.Lerp(data.Thickness, 0f, trail.ThicknessLerpAmount);
                }
            }

            trail.Accumulator += frameTime;

            if (trail.Frequency <= 0f)
            {
                if (trail.Accumulator <= trail.Lifetime)
                    continue;

                trail.Accumulator = 0f;

                if (trail.TrailData.Count == 0f)
                {
                    if (IsClientSide(uid))
                        QueueDel(uid);
                    continue;
                }

                trail.TrailData.RemoveAt(0);

                continue;
            }

            if (trail.Accumulator <= trail.Frequency)
                continue;

            trail.Accumulator = 0f;

            var (position, rotation) = _transform.GetWorldPositionRotation(xform, xformQuery);
            if (trail.TrailData.Count < trail.Lifetime / trail.Frequency)
            {
                trail.TrailData.Add(new TrailData(position, rotation, trail.Color, trail.LineThickness));
            }
            else
            {
                if (trail.CurIndex >= trail.TrailData.Count)
                    trail.CurIndex = 0;

                var data = trail.TrailData[trail.CurIndex];

                data.Color = trail.Color;
                data.Position = position;
                data.Angle = rotation;
                data.Thickness = trail.LineThickness;

                if (trail.Sprite == null)
                {
                    if (trail.ColorLerpAmount <= 0f || trail.ThicknessLerpAmount <= 0f)
                        continue;

                    for (var i = 0; i + 1 < trail.TrailData.Count; i++)
                    {
                        // Push first element to the end of the list
                        (trail.TrailData[i], trail.TrailData[i + 1]) = (trail.TrailData[i + 1], trail.TrailData[i]);
                    }
                }
                else
                    trail.CurIndex++;
            }
        }
    }
}
