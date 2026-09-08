using System.Numerics;
using Content.Shared.Camera;
using Content.Shared.Movement.Components;

namespace Content.Goobstation.Shared.Cinematic;

/// <summary>
/// Decides who is watching and pulls their camera.
/// </summary>
public sealed partial class SharedCinematicSystem
{
    private void UpdateEngagement(Entity<CinematicComponent> ent, CinematicPrototype timeline, EntityUid? viewer)
    {
        var comp = ent.Comp;

        if (viewer is not { } uid)
            comp.Engaged = false;
        else if (ent.Owner == uid)
            comp.Engaged = true;
        else if (!timeline.PullsOtherCameras)
            comp.Engaged = false;
        else if (!comp.Engaged && GetStrength(comp, timeline) > 0f)
            comp.Engaged = _examine.InRangeUnOccluded(uid, ent, timeline.ViewerRange);
    }

    private void UpdateCamera(Entity<CinematicComponent> ent, CinematicPrototype timeline, EntityUid? viewer, float frameTime)
    {
        var comp = ent.Comp;
        var target = comp.Engaged ? GetStrength(comp, timeline) : 0f;

        comp.Pan = target > 0f && viewer is { } uid
            ? GetPan(ent, timeline, uid, target)
            : Vector2.Lerp(comp.Pan, Vector2.Zero, MathF.Min(1f, frameTime * timeline.PanReturnRate));

        var rate = target > comp.Strength ? timeline.EngageRate : timeline.DisengageRate;
        comp.Strength = MathHelper.Lerp(comp.Strength, target, MathF.Min(1f, frameTime * rate));

        if (target <= 0f && comp.Strength < 0.005f)
        {
            comp.Strength = 0f;
            comp.Pan = Vector2.Zero;
        }

        if (!comp.Engaged && comp.Strength <= 0f)
            return;

        // Prevents a lot of manual timing.
        var remaining = MathF.Max(0f, (float) (comp.EndTime - _timing.CurTime).TotalSeconds);
        var ev = new CinematicUpdatedEvent(comp.Strength, remaining);
        RaiseLocalEvent(ent, ref ev);
        comp.EyeOffset = comp.Pan + ev.EyeOffset;
    }

    private Vector2 GetPan(Entity<CinematicComponent> ent, CinematicPrototype timeline, EntityUid viewer, float strength)
    {
        var delta = _transform.GetWorldPosition(ent) - _transform.GetWorldPosition(viewer);
        if (delta.Length() > timeline.MaxPanDistance)
            delta = Vector2.Normalize(delta) * timeline.MaxPanDistance;

        return delta * timeline.CameraPull * strength;
    }

    private void OnGetEyeOffset(Entity<ContentEyeComponent> ent, ref GetEyeOffsetEvent args)
    {
        if (ent.Owner != _player.LocalEntity)
            return;

        var query = EntityQueryEnumerator<CinematicComponent>();
        while (query.MoveNext(out _, out var comp))
            args.Offset += comp.EyeOffset;
    }
}
