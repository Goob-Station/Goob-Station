using System.Numerics;
using Content.Goobstation.Shared.Cinematic;
using Content.Shared._DV.CCVars;
using Content.Shared.Camera;
using Robust.Client.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Cinematic;

/// <summary>
/// Shakes the local players camera.
/// </summary>
public sealed class CinematicShakeSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EyeComponent, GetEyeOffsetEvent>(OnGetEyeOffset);
        SubscribeLocalEvent<CinematicShakeComponent, CinematicUpdatedEvent>(OnCinematicUpdated);
    }

    private void OnCinematicUpdated(Entity<CinematicShakeComponent> ent, ref CinematicUpdatedEvent args) =>
        ent.Comp.Strength = args.Strength;

    private void OnGetEyeOffset(Entity<EyeComponent> ent, ref GetEyeOffsetEvent args)
    {
        if (ent.Owner != _player.LocalEntity || _cfg.GetCVar(DCCVars.NoVisionFilters))
            return;

        var query = EntityQueryEnumerator<CinematicShakeComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            var strength = comp.Strength * GetViewerFactor(uid, ent, comp.Range);
            if (strength <= 0f)
                continue;

            var slice = Slice(comp);
            args.Offset += new Vector2(Jitter(slice, 0), Jitter(slice, 1)) * (comp.Amplitude * strength);
        }
    }

    public float GetZoomJitter(CinematicShakeComponent shake) =>
        1f + Jitter(Slice(shake), 2) * shake.ZoomJitter * shake.Strength;

    public float GetViewerFactor(EntityUid source, EntityUid viewer, float range)
    {
        if (source == viewer)
            return 1f;

        if (range <= 0f || Transform(source).MapID != Transform(viewer).MapID)
            return 0f;

        var delta = _transform.GetWorldPosition(source) - _transform.GetWorldPosition(viewer);
        return Math.Clamp(1f - delta.Length() / range, 0f, 1f);
    }

    private int Slice(CinematicShakeComponent shake) =>
        (int) (_timing.CurTime.TotalSeconds * shake.KeyframeRate);

    private static float Jitter(int slice, int salt) =>
        new Random(HashCode.Combine(slice, salt)).NextSingle() - 0.5f;
}
