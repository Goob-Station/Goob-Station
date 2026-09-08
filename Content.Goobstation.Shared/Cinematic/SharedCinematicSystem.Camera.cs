using System.Numerics;
using Content.Shared.Camera;
using Content.Shared.Movement.Components;

namespace Content.Goobstation.Shared.Cinematic;

/// <summary>
/// Handles camera pulling.
/// </summary>
public sealed partial class SharedCinematicSystem
{
    private void UpdateCamera(Entity<CinematicComponent> ent, EntityUid? viewer, float frameTime)
    {
        var comp = ent.Comp;
        var target = GetTargetStrength(ent, viewer);
        var pulling = target > 0f;

        comp.Pan = pulling && viewer is { } uid
            ? GetPan(ent, uid, target)
            : Vector2.Lerp(comp.Pan, Vector2.Zero, MathF.Min(1f, frameTime * comp.PanReturnRate));

        var rate = target > comp.Strength ? comp.EngageRate : comp.DisengageRate;
        comp.Strength = MathHelper.Lerp(comp.Strength, target, MathF.Min(1f, frameTime * rate));

        if (!pulling && comp.Strength < 0.005f)
        {
            comp.Strength = 0f;
            comp.Pan = Vector2.Zero;
        }

        if (!comp.Engaged)
            return;

        // Prevents a lot of manual timing.
        var ev = new CinematicUpdatedEvent(comp.Strength);
        RaiseLocalEvent(ent, ref ev);
        comp.EyeOffset = comp.Pan + ev.EyeOffset;
    }

    private float GetTargetStrength(Entity<CinematicComponent> ent, EntityUid? viewer)
    {
        if (viewer is not { } uid)
            return 0f;

        var strength = GetStrength(ent.Comp);
        if (strength <= 0f)
            return 0f;

        if (!ent.Comp.Engaged)
            ent.Comp.Engaged = ShouldEngage(ent, uid);

        return ent.Comp.Engaged ? strength : 0f;
    }

    private Vector2 GetPan(Entity<CinematicComponent> ent, EntityUid viewer, float strength)
    {
        var delta = _transform.GetWorldPosition(ent) - _transform.GetWorldPosition(viewer);
        if (delta.Length() > ent.Comp.MaxPanDistance)
            delta = Vector2.Normalize(delta) * ent.Comp.MaxPanDistance;

        return delta * ent.Comp.CameraPull * strength;
    }

    private bool ShouldEngage(Entity<CinematicComponent> ent, EntityUid viewer)
    {
        if (ent.Owner == viewer)
            return true;

        if (!ent.Comp.PullsOtherCameras)
            return false;

        return _examine.InRangeUnOccluded(viewer, ent, ent.Comp.ViewerRange);
    }

    private bool IsWatching(Entity<CinematicComponent> ent) =>
        ent.Owner == _player.LocalEntity || ent.Comp.Engaged;

    private void OnGetEyeOffset(Entity<ContentEyeComponent> ent, ref GetEyeOffsetEvent args)
    {
        if (ent.Owner != _player.LocalEntity)
            return;

        var query = EntityQueryEnumerator<CinematicComponent>();
        while (query.MoveNext(out _, out var comp))
            args.Offset += comp.EyeOffset;
    }
}
