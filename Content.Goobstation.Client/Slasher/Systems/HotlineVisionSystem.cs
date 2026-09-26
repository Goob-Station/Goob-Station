using Content.Client.Eye;
using Content.Goobstation.Client.Slasher.Overlays;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared._DV.CCVars;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.Slasher.Systems;

/// <summary>
/// Gives the user drunk cam / a rainbow esq fov look.
/// </summary>
public sealed class HotlineVisionSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;

    private HotlineVisionOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HotlineVisionComponent, ComponentInit>(OnHotlineVisionInit);
        SubscribeLocalEvent<HotlineVisionComponent, ComponentShutdown>(OnHotlineVisionShutdown);
        SubscribeLocalEvent<HotlineVisionComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<HotlineVisionComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        Subs.CVar(_cfg, DCCVars.NoVisionFilters, OnNoVisionFiltersChanged);

        _overlay = new();

        UpdatesAfter.Add(typeof(EyeLerpingSystem));
    }

    private void OnHotlineVisionInit(EntityUid uid, HotlineVisionComponent component, ComponentInit args)
    {
        if (uid == _player.LocalEntity && !_cfg.GetCVar(DCCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnHotlineVisionShutdown(EntityUid uid, HotlineVisionComponent component, ComponentShutdown args)
    {
        if (uid == _player.LocalEntity)
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnPlayerAttached(EntityUid uid, HotlineVisionComponent component, LocalPlayerAttachedEvent args)
    {
        if (!_cfg.GetCVar(DCCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(EntityUid uid, HotlineVisionComponent component, LocalPlayerDetachedEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnNoVisionFiltersChanged(bool enabled)
    {
        if (enabled)
            _overlayMan.RemoveOverlay(_overlay);
        else if (HasComp<HotlineVisionComponent>(_player.LocalEntity))
            _overlayMan.AddOverlay(_overlay);
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        if (_player.LocalEntity is not { } local
            || !TryComp<HotlineVisionComponent>(local, out var vision)
            || !TryComp<EyeComponent>(local, out var eye))
            return;

        var leanTarget = 0f;
        var moving = false;
        if (TryComp<PhysicsComponent>(local, out var physics)
            && physics.LinearVelocity.LengthSquared() > 0.05f)
        {
            moving = true;

            var eyeRotation = eye.Rotation;
            var screenVelocity = eyeRotation.RotateVec(physics.LinearVelocity);
            leanTarget = vision.MaxLeanDegrees * Math.Clamp(screenVelocity.X / physics.LinearVelocity.Length(), -1f, 1f);
        }

        vision.MoveFactor = MathHelper.Lerp(vision.MoveFactor, moving ? 1f : 0f, Math.Min(1f, vision.MoveBlendSpeed * frameTime));
        vision.SwayClock += frameTime * MathHelper.Lerp(vision.IdleSwayRate, vision.MoveSwayRate, vision.MoveFactor);
        vision.Lean = MathHelper.Lerp(vision.Lean, leanTarget, Math.Min(1f, vision.LeanLerpSpeed * frameTime));

        var sway = MathF.Sin(vision.SwayClock * 0.61f) * 0.6f + MathF.Sin(vision.SwayClock * 0.37f + 1.7f) * 0.4f;
        var amplitude = MathHelper.Lerp(vision.IdleSwayDegrees, vision.MoveSwayDegrees, vision.MoveFactor);
        _eye.SetRotation(local, eye.Rotation + Angle.FromDegrees(sway * amplitude + vision.Lean), eye);
    }
}
