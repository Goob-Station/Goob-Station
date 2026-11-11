using System.Numerics;
using Content.Goobstation.Shared.Nullspace;

namespace Content.Goobstation.Client.Nullspace;

public sealed partial class NullspaceSystem
{
    private const float Cell = NullspaceParams.Cell;
    private const float BaseFov = 72f;
    private static readonly TimeSpan QuitDelay = TimeSpan.FromSeconds(0.38);

    private readonly NullspaceTracks _tracks = new();

    private Vector2 _lastPosition;
    private bool _hasLastPosition;
    private Vector2 _velocity;
    private float _stepPhase;
    private int _lastStep;
    private float _bobAmp;
    private float _roll;
    private float _shake;
    private float _fov = BaseFov;
    private float _fear;
    private float _flash;
    private float _flashStutter;
    private Vector3 _flashDirection = Vector3.UnitY;
    private float _gameTime;
    private bool _dying;
    private bool _quitting;
    private TimeSpan _quitAt;
    private float _clownDistance = float.PositiveInfinity;
    private bool _clownCanSee;

    private void ResetView(EntityUid uid)
    {
        _yaw = WrapAngle((float) (_transform.GetWorldRotation(uid).Theta - Math.PI));
        _pitch = 0f;
        _sentYaw = float.NaN;
        _sentPitch = float.NaN;
        _startSent = false;
        _hasLastPosition = false;
        _velocity = Vector2.Zero;
        _stepPhase = 0f;
        _lastStep = 0;
        _bobAmp = 0f;
        _roll = 0f;
        _shake = 0f;
        _fov = BaseFov;
        _fear = 0f;
        _flash = 0f;
        _flashStutter = 0f;
        _flashDirection = new Vector3(-MathF.Sin(_yaw), MathF.Cos(_yaw), 0f);
        _gameTime = 0f;
        _dying = false;
        _quitting = false;
        _tracks.Clown.Uid = null;
        _tracks.Clown.Phase = 0f;
        _tracks.Players.Clear();
        _clownDistance = float.PositiveInfinity;
        _clownCanSee = false;
        ResetAudio();
    }

    private void UpdateView(Entity<NullspacePlayerComponent> player, Entity<NullspaceMapComponent> map, NullspaceMaze maze, float dt)
    {
        var started = map.Comp.StartTime != null;
        _gameTime = map.Comp.StartTime is { } start ? (float) (_timing.CurTime - start).TotalSeconds : 0f;
        var time = (float) (_timing.RealTime.TotalSeconds % 3600.0);

        var position = _transform.GetWorldPosition(player) * Cell;
        if (!_hasLastPosition || (position - _lastPosition).Length() > Cell * 2f)
        {
            _lastPosition = position;
            _hasLastPosition = true;
        }

        var displacement = position - _lastPosition;
        _lastPosition = position;
        var realSpeed = displacement.Length() / dt;
        _velocity = Vector2.Lerp(_velocity, displacement / dt, 1f - MathF.Exp(-20f * dt));

        var sprinting = player.Comp.Sprinting;
        if (started && !player.Comp.Frozen)
        {
            var stride = sprinting ? NullspaceParams.SprintStride : NullspaceParams.WalkStride;
            _stepPhase += realSpeed * dt * MathF.PI / stride;
            var stepIndex = (int) MathF.Floor(_stepPhase / MathF.PI);
            if (stepIndex != _lastStep)
            {
                _lastStep = stepIndex;
                if (realSpeed > 0.4f)
                    Footstep(sprinting);
            }
        }

        var flatRight = new Vector2(MathF.Cos(_yaw), MathF.Sin(_yaw));
        var targetAmp = realSpeed < 0.2f ? 0f : sprinting ? 0.075f : 0.038f;
        _bobAmp = Damp(_bobAmp, targetAmp, 8f, dt);
        _roll = Damp(_roll, -Vector2.Dot(_velocity, flatRight) * 0.012f, 6f, dt);

        UpdateClown(player, map, maze, position);

        if (started)
        {
            UpdateFear(dt);
            UpdateBody(player.Comp, dt);
            UpdateAmbience(map, position, dt);
        }

        var bobY = -_bobAmp * (1f - MathF.Abs(MathF.Sin(_stepPhase)));
        var swayX = _bobAmp * 0.55f * MathF.Cos(_stepPhase);
        var idle = MathF.Sin(time * 1.3f) * 0.006f * (1f + (1f - player.Comp.Stamina) * 2f);
        _shake = Damp(_shake, _fear > 0.5f ? (_fear - 0.5f) * 0.02f : 0f, 4f, dt);

        var camera = new Vector3(
            position.X + flatRight.X * swayX + (_random.NextFloat() - 0.5f) * _shake,
            position.Y + flatRight.Y * swayX,
            NullspaceParams.Eye + bobY + idle + (_random.NextFloat() - 0.5f) * _shake);

        var (forward, right, up, _, _) = Basis(_yaw, _pitch + bobY * 0.4f, _roll + swayX * 0.25f);

        _fov = Damp(_fov, BaseFov + (sprinting ? 4f : 0f) - _fear * 8f, 4f, dt);
        _flashDirection = Vector3.Normalize(Vector3.Lerp(_flashDirection, forward, 1f - MathF.Exp(-11f * dt)));
        _flashStutter -= dt;

        var darkness = NullspaceParams.Darkness(_gameTime);
        if (started && _random.NextFloat() < dt * darkness * 0.6f)
            _flashStutter = MathF.Max(_flashStutter, _random.NextFloat(0.05f, 0.3f));

        var flashlight = Flashlight(darkness);

        if (_dying)
        {
            _flashStutter = 0f;
            _flash *= MathF.Pow(0.85f, dt * 60f);
            if (!_quitting && _timing.RealTime >= _quitAt)
            {
                _quitting = true;
                _game.Shutdown();
            }
        }

        _eye.SetRotation(player, new Angle(-_yaw));

        var frame = _frame;
        FillView(frame, maze, camera, forward, right, up, _fov, time);
        frame.Fear = _fear;
        frame.Exhaustion = Exhaustion(player.Comp);
        frame.Flash = _flash;
        frame.FadeIn = started ? MathF.Min(1f, _gameTime / 3f) : 0.3f;

        (_nearestLit, _nearestLevel) = CollectLights(frame, map.Comp, maze, position, time,
            _tracks.Clown.Uid != null ? _tracks.Clown.Position : null, true);
        FillActors(frame, _tracks, map, player, camera, forward, right, up, _flashDirection,
            _flashStutter > 0f ? flashlight * 0.04f : flashlight, flashlight, _dying);
    }

    private void UpdateClown(Entity<NullspacePlayerComponent> player, Entity<NullspaceMapComponent> map, NullspaceMaze maze, Vector2 playerPos)
    {
        if (player.Comp.Frozen && !_dying)
            StartDying();

        var clown = _tracks.Clown;
        if (TrackClown(clown, map))
            OnClownArrived();

        (_clownDistance, _clownCanSee) = ClownSight(clown, maze, playerPos);
        UpdateChaseMusic(clown.Uid != null && clown.State == NullspaceClownState.Chase && clown.Target == player.Owner);
    }

    private void UpdateFear(float dt)
    {
        var target = FearTarget(_tracks.Clown, _viewer, _clownDistance, _clownCanSee);
        if (_tracks.Clown.Uid != null && _clownDistance < 12f
            && _random.NextFloat() < dt * ((12f - _clownDistance) / 12f) * 4f)
        {
            _flashStutter = MathF.Max(_flashStutter, _random.NextFloat(0.04f, 0.16f));
        }

        if (_dying)
            target = 1f;

        _fear = Damp(_fear, target, target > _fear ? 3f : 0.45f, dt);
    }

    private static float FearTarget(NullspaceClownTrack track, EntityUid? viewer, float distance, bool canSee)
    {
        if (track.Uid == null)
            return 0f;

        var target = MathF.Max(0f, 1f - distance / 22f);
        if (canSee)
            target = MathF.Max(target, 0.55f + 0.45f * MathF.Max(0f, 1f - distance / 20f));
        if (track.State == NullspaceClownState.Chase && track.Target == viewer)
            target = MathF.Max(target, 0.45f);

        return target;
    }

    private static float Flashlight(float darkness)
    {
        return FlashlightIntensity * (1f - NullspaceParams.DarknessFlashlightDim * darkness);
    }

    private static float Exhaustion(NullspacePlayerComponent player)
    {
        return player.Exhausted ? 1f : MathF.Max(0f, 0.5f - player.Stamina) * 1.2f;
    }

    private void StartDying()
    {
        _dying = true;
        _flash = 0.12f;
        _quitAt = _timing.RealTime + QuitDelay;
        PlayDeath();
    }

    private static float Damp(float current, float target, float rate, float dt)
    {
        return current + (target - current) * (1f - MathF.Exp(-rate * dt));
    }
}
