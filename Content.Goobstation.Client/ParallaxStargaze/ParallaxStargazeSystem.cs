// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.ParallaxStargaze;
using Content.Shared.Mobs.Components;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.ParallaxStargaze;

public sealed class ParallaxStargazeSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public EntityUid? ActiveCampfire { get; private set; }
    public float Progress { get; private set; }
    public readonly List<EntityUid> Exempt = new();

    private EntityUid? _lastCampfire;
    private float _stillTime;
    private EntityUid? _music;

    public override void Initialize()
    {
        base.Initialize();
        _overlay.AddOverlay(new ParallaxStargazeOverlay());
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlay.RemoveOverlay<ParallaxStargazeOverlay>();
        StopMusic();
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var campfire = ResolveCampfire();
        var active = false;

        if (campfire is { } fire && _player.LocalEntity is { } player)
        {
            if (IsMoving(player, fire.Comp.MoveThreshold))
                _stillTime = 0f;
            else
                _stillTime += frameTime;

            active = _stillTime >= fire.Comp.StillTime;
            _lastCampfire = fire.Owner;
        }
        else
        {
            _stillTime = 0f;
        }

        var fadeTime = campfire?.Comp.FadeTime ?? 2.5f;
        var step = fadeTime > 0f ? frameTime / fadeTime : 1f;
        Progress = Math.Clamp(Progress + (active ? step : -step), 0f, 1f);

        ActiveCampfire = Progress > 0f ? _lastCampfire : null;

        BuildExempt(campfire?.Comp.MobRadius ?? 5f);
        UpdateMusic(active, campfire?.Comp.Music);
    }

    private void BuildExempt(float mobRadius)
    {
        Exempt.Clear();

        if (ActiveCampfire is not { } fire || !TryComp(fire, out TransformComponent? fireXform))
            return;

        Exempt.Add(fire);

        var player = _player.LocalEntity;
        if (player is { } local)
            Exempt.Add(local);

        var firePos = _transform.GetWorldPosition(fire);
        var radiusSq = mobRadius * mobRadius;

        var query = EntityQueryEnumerator<MobStateComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var xform))
        {
            if (uid == player || xform.MapID != fireXform.MapID)
                continue;

            if ((_transform.GetWorldPosition(uid) - firePos).LengthSquared() <= radiusSq)
                Exempt.Add(uid);
        }
    }

    private Entity<ParallaxStargazeComponent>? ResolveCampfire()
    {
        if (_player.LocalEntity is not { } player)
            return null;

        var playerXform = Transform(player);
        if (playerXform.MapID == MapId.Nullspace)
            return null;

        var playerPos = _transform.GetWorldPosition(player);
        Entity<ParallaxStargazeComponent>? best = null;
        var bestDist = float.MaxValue;

        var query = EntityQueryEnumerator<ParallaxStargazeComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            if (xform.MapID != playerXform.MapID)
                continue;

            var dist = (_transform.GetWorldPosition(uid) - playerPos).Length();
            if (dist <= comp.ActivationRadius && dist < bestDist)
            {
                bestDist = dist;
                best = (uid, comp);
            }
        }

        return best;
    }

    private bool IsMoving(EntityUid player, float threshold)
    {
        return TryComp<PhysicsComponent>(player, out var phys) && phys.LinearVelocity.Length() > threshold;
    }

    private void UpdateMusic(bool active, SoundSpecifier? music)
    {
        if (active && _music == null && music != null)
        {
            var played = _audio.PlayGlobal(music, Filter.Local(), false, AudioParams.Default.WithLoop(true).WithVolume(-30f));
            _music = played?.Entity;
        }

        if (_music is { } uid)
        {
            if (Progress <= 0f)
                StopMusic();
            else
                _audio.SetVolume(uid, -30f + Progress * 24f);
        }
    }

    private void StopMusic()
    {
        if (_music is { } uid)
            QueueDel(uid);
        _music = null;
    }
}
