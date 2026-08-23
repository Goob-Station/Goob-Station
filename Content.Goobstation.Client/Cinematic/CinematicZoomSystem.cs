using Content.Client.Eye;
using Content.Goobstation.Shared.Cinematic;
using Content.Shared._DV.CCVars;
using Content.Shared.Movement.Components;
using Robust.Client.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Cinematic;

/// <summary>
/// Zooms the local player's camera.
/// </summary>
public sealed class CinematicZoomSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly CinematicShakeSystem _shake = default!;

    public override void Initialize()
    {
        base.Initialize();

        UpdatesAfter.Add(typeof(EyeLerpingSystem));
        UpdatesBefore.Add(typeof(SharedEyeSystem));

        SubscribeLocalEvent<CinematicZoomComponent, CinematicUpdatedEvent>(OnCinematicUpdated);
    }

    private void OnCinematicUpdated(Entity<CinematicZoomComponent> ent, ref CinematicUpdatedEvent args) =>
        ent.Comp.Strength = args.Strength;

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var player = _player.LocalEntity;

        if (player is not { } viewer
            || !TryComp<EyeComponent>(viewer, out var eyeComp)
            || !TryComp<ContentEyeComponent>(viewer, out var contentEye))
            return;

        var query = EntityQueryEnumerator<CinematicZoomComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_cfg.GetCVar(DCCVars.NoVisionFilters))
                continue;

            var strength = comp.Strength * _shake.GetViewerFactor(uid, viewer, comp.Range);
            if (strength <= 0f)
                continue;

            var factor = MathHelper.Lerp(1f, comp.ZoomFactor, strength);

            if (TryComp<CinematicShakeComponent>(uid, out var shake))
            {
                var rand = new Random((int) (_timing.CurTime.TotalSeconds * shake.KeyframeRate));
                factor *= 1f + (rand.NextSingle() - 0.5f) * shake.ZoomJitter * shake.Strength;
            }

            _eye.SetZoom(viewer, contentEye.TargetZoom * factor, eyeComp);
        }
    }
}
