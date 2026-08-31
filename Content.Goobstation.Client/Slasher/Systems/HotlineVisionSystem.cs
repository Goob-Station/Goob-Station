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
/// Registers the hotline vision overlay / drives the drunk cam effect.
/// </summary>
public sealed class HotlineVisionSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;

    private HotlineVisionOverlay _overlay = default!;

    private const float IdleSwayDegrees = 0.85f;
    private const float MoveSwayDegrees = 2.25f;
    private const float IdleSwayRate = 1.0f;
    private const float MoveSwayRate = 2.1f;
    private const float MaxLeanDegrees = 2.5f;
    private const float LeanLerpSpeed = 3.5f;
    private const float MoveBlendSpeed = 2.5f;

    private float _swayClock;
    private float _lean;
    private float _moveFactor;

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
            || !HasComp<HotlineVisionComponent>(local)
            || !TryComp<EyeComponent>(local, out var eye))
        {
            _lean = 0f;
            _moveFactor = 0f;
            return;
        }

        var leanTarget = 0f;
        var moving = false;

        if (TryComp<PhysicsComponent>(local, out var physics)
            && physics.LinearVelocity.LengthSquared() > 0.05f)
        {
            moving = true;

            var eyeRot = eye.Rotation;
            var screenVel = eyeRot.RotateVec(physics.LinearVelocity);
            leanTarget = MaxLeanDegrees * Math.Clamp(screenVel.X / physics.LinearVelocity.Length(), -1f, 1f);
        }

        _moveFactor = MathHelper.Lerp(_moveFactor, moving ? 1f : 0f, Math.Min(1f, MoveBlendSpeed * frameTime));
        _swayClock += frameTime * MathHelper.Lerp(IdleSwayRate, MoveSwayRate, _moveFactor);
        _lean = MathHelper.Lerp(_lean, leanTarget, Math.Min(1f, LeanLerpSpeed * frameTime));

        var sway = MathF.Sin(_swayClock * 0.61f) * 0.6f + MathF.Sin(_swayClock * 0.37f + 1.7f) * 0.4f;
        var amplitude = MathHelper.Lerp(IdleSwayDegrees, MoveSwayDegrees, _moveFactor);

        var tilt = sway * amplitude + _lean;
        _eye.SetRotation(local, eye.Rotation + Angle.FromDegrees(tilt), eye);
    }
}
