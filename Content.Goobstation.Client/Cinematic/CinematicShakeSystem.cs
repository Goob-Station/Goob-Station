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

    private bool _noVisionFilters;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EyeComponent, GetEyeOffsetEvent>(OnGetEyeOffset);
        SubscribeLocalEvent<CinematicShakeComponent, CinematicUpdatedEvent>(OnCinematicUpdated);

        Subs.CVar(_cfg, DCCVars.NoVisionFilters, OnNoVisionFiltersChanged, true);
    }

    private void OnCinematicUpdated(Entity<CinematicShakeComponent> ent, ref CinematicUpdatedEvent args) =>
        ent.Comp.Strength = args.Strength;

    private void OnNoVisionFiltersChanged(bool enabled) => _noVisionFilters = enabled;

    private void OnGetEyeOffset(Entity<EyeComponent> ent, ref GetEyeOffsetEvent args)
    {
        if (_noVisionFilters || ent.Owner != _player.LocalEntity)
            return;

        var query = EntityQueryEnumerator<CinematicShakeComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            var strength = comp.Strength * GetViewerFactor(uid, ent, comp.Range);
            if (strength <= 0f)
                continue;

            var slice = (int) (_timing.CurTime.TotalSeconds * comp.KeyframeRate);
            var rand = new Random(slice);
            args.Offset += new Vector2(rand.NextSingle() - 0.5f, rand.NextSingle() - 0.5f)
                * (comp.Amplitude * strength);
        }
    }

    public float GetViewerFactor(EntityUid source, EntityUid viewer, float range)
    {
        if (source == viewer)
            return 1f;

        if (range <= 0f || Transform(source).MapID != Transform(viewer).MapID)
            return 0f;

        var delta = _transform.GetWorldPosition(source) - _transform.GetWorldPosition(viewer);
        return Math.Clamp(1f - delta.Length() / range, 0f, 1f);
    }
}
