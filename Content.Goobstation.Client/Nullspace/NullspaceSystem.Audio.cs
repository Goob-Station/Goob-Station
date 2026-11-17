using System.Numerics;
using Content.Goobstation.Shared.Nullspace;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.Nullspace;

public sealed partial class NullspaceSystem
{
    [Dependency] private SharedAudioSystem _audio = default!;

    private const float HumScale = 0.12f;
    private const float SilenceFall = 7f;
    private const float SilenceRise = 0.35f;

    private readonly Dictionary<EntityUid, Fade> _fades = new();
    private readonly Dictionary<EntityUid, float> _loopGains = new();
    private readonly List<EntityUid> _finishedFades = new();

    private EntityUid? _hum;
    private EntityUid? _drone;
    private EntityUid? _vent;
    private EntityUid? _dread;
    private EntityUid? _chase;
    private bool _ambienceStarted;
    private bool _clownArrived;
    private bool _chasing;
    private float _breathCooldown;
    private float _heartTimer;
    private float _nextAmbient;
    private float _lastBuzz = float.NegativeInfinity;
    private bool _wasExhausted;
    private float _silence = 1f;
    private float _silenceUntil = float.NegativeInfinity;

    private record struct Fade(float From, float To, float Start, float Duration);

    private void ResetAudio()
    {
        StopAudio();
        _ambienceStarted = false;
        _clownArrived = false;
        _chasing = false;
        _breathCooldown = 0f;
        _heartTimer = 0f;
        _nextAmbient = _random.NextFloat(8f, 16f);
        _lastBuzz = float.NegativeInfinity;
        _wasExhausted = false;
        _silence = 1f;
        _silenceUntil = float.NegativeInfinity;
    }

    private void StopAudio()
    {
        foreach (var loop in new[] { _hum, _drone, _vent, _dread, _chase })
        {
            _audio.Stop(loop);
        }

        _hum = _drone = _vent = _dread = _chase = null;
        _fades.Clear();
        _loopGains.Clear();
    }

    [SubscribeNetworkEvent]
    private void OnSilence(NullspaceSilenceEvent ev)
    {
        if (_viewer == null || !float.IsFinite(ev.Seconds))
            return;

        _silenceUntil = MathF.Max(_silenceUntil, _gameTime + Math.Clamp(ev.Seconds, 0f, 6f));
    }

    private bool Silenced => _gameTime < _silenceUntil;

    private void UpdateAmbience(Entity<NullspaceMapComponent> map, Vector2 playerPos, float dt)
    {
        if (!_ambienceStarted)
            StartAmbience(map, playerPos);

        _silence = Silenced
            ? MathF.Max(0f, _silence - SilenceFall * dt)
            : MathF.Min(1f, _silence + SilenceRise * dt);
        UpdateLoops();

        if (Silenced)
            _nextAmbient = MathF.Max(_nextAmbient, _silenceUntil + _random.NextFloat(4f, 8f));

        if (_gameTime >= _nextAmbient)
        {
            var angle = _random.NextFloat(0f, MathF.Tau);
            var distance = _random.NextFloat(8f, 20f);
            var position = playerPos + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * distance;
            var ambient = _audio.PlayStatic(NullspaceAudio.Ambient,
                Filter.Local(),
                ToCoordinates(map, position),
                false,
                NullspaceAudio.Positional(0.8f));
            Unoccluded(ambient);
            ApplyReverb(ambient);
            _nextAmbient = _gameTime + _random.NextFloat(15f, 40f);
        }

        if (_hum is { } hum && !TerminatingOrDeleted(hum))
        {
            if (_nearestLit is { } lit)
                _transform.SetCoordinates(hum, ToCoordinates(map, new Vector2(lit.X, lit.Y)));

            _audio.SetGain(hum, _nearestLevel * HumScale * _silence);
        }
    }

    private void StartAmbience(EntityUid map, Vector2 playerPos)
    {
        _ambienceStarted = true;
        var hum = _audio.PlayStatic(NullspaceAudio.Hum,
            Filter.Local(),
            ToCoordinates(map, playerPos),
            false,
            NullspaceAudio.Positional(0.0001f, 1f, 1f, 1.4f, 6f, true));
        Unoccluded(hum);
        _hum = hum?.Entity;
        _drone = PlayLoop(NullspaceAudio.Drone, 0.35f);
        _vent = PlayLoop(NullspaceAudio.Vent, 0.1f, 0.85f);
        _dread = PlayLoop(NullspaceAudio.Dread, 0.0001f);
        _chase = PlayLoop(NullspaceAudio.Chase, 0.0001f);
        FadeTo(_dread, 0.22f, 8f);
    }

    private EntityUid? PlayLoop(SoundSpecifier sound, float gain, float pitch = 1f)
    {
        var loop = _audio.PlayGlobal(sound, Filter.Local(), false, NullspaceAudio.Global(gain, pitch, true))?.Entity;
        if (loop is { } uid)
            _loopGains[uid] = gain;

        return loop;
    }

    private void FadeTo(EntityUid? uid, float to, float seconds)
    {
        if (uid is not { } audio || !_loopGains.TryGetValue(audio, out var from))
            return;

        _fades[audio] = new Fade(from, to, (float) _timing.RealTime.TotalSeconds, MathF.Max(seconds, 0.001f));
    }

    private void UpdateLoops()
    {
        var now = (float) _timing.RealTime.TotalSeconds;
        _finishedFades.Clear();
        foreach (var (uid, fade) in _fades)
        {
            var t = Math.Clamp((now - fade.Start) / fade.Duration, 0f, 1f);
            _loopGains[uid] = fade.From + (fade.To - fade.From) * t;
            if (t >= 1f)
                _finishedFades.Add(uid);
        }

        foreach (var uid in _finishedFades)
        {
            _fades.Remove(uid);
        }

        _finishedFades.Clear();
        foreach (var (uid, gain) in _loopGains)
        {
            if (TerminatingOrDeleted(uid))
            {
                _finishedFades.Add(uid);
                continue;
            }

            _audio.SetGain(uid, uid == _chase ? gain : gain * _silence);
        }

        foreach (var uid in _finishedFades)
        {
            _loopGains.Remove(uid);
            _fades.Remove(uid);
        }
    }

    private void OnClownArrived()
    {
        if (_clownArrived)
            return;

        _clownArrived = true;
        FadeTo(_dread, 0f, 0.25f);
        FadeTo(_vent, 0f, 0.4f);
    }

    private void UpdateChaseMusic(bool chasing)
    {
        if (_chasing == chasing)
            return;

        _chasing = chasing;
        FadeTo(_chase, chasing ? 0.45f : 0f, chasing ? 0.8f : 5f);
    }

    private void Footstep(bool sprinting)
    {
        var step = _audio.PlayGlobal(sprinting ? NullspaceAudio.Run : NullspaceAudio.Step,
            Filter.Local(),
            false,
            NullspaceAudio.Global(sprinting ? 0.75f : 0.42f, _random.NextFloat(0.92f, 1.04f)));

        if (sprinting)
            ApplyReverb(step);
    }

    private void UpdateBody(NullspacePlayerComponent player, float dt)
    {
        if (player.Exhausted && !_wasExhausted)
        {
            _audio.PlayGlobal(NullspaceAudio.BreathHeavy, Filter.Local(), false, NullspaceAudio.Global(0.9f));
            _breathCooldown = 5.5f;
        }

        _wasExhausted = player.Exhausted;

        _breathCooldown -= dt;
        if (_breathCooldown <= 0f)
        {
            var stamina = player.Stamina;
            var gain = stamina < 0.5f ? MathF.Min((0.7f - stamina) / 0.7f, 1f) : 0f;
            gain = MathF.Max(gain, _fear > 0.35f ? _fear * 0.8f : 0f);
            if (gain > 0.05f)
            {
                _audio.PlayGlobal(NullspaceAudio.Breath,
                    Filter.Local(),
                    false,
                    NullspaceAudio.Global(gain * 0.85f, _random.NextFloat(0.95f, 1.05f)));
                _breathCooldown = 0.85f + (1f - gain) * 0.9f + _random.NextFloat() * 0.25f;
            }
            else
            {
                _breathCooldown = 0.5f;
            }
        }

        _heartTimer -= dt;
        if (_fear > 0.12f && _heartTimer <= 0f)
        {
            _audio.PlayGlobal(NullspaceAudio.Heartbeat,
                Filter.Local(),
                false,
                NullspaceAudio.Global(MathF.Min(1f, _fear * 1.2f), 0.9f + _fear * 0.15f));
            _heartTimer = 1.05f - _fear * 0.62f;
        }
    }

    private void OnFlickerOff(Vector2 position)
    {
        if (!_ambienceStarted
            || Silenced
            || _viewer is not { } uid
            || Transform(uid).MapUid is not { } map
            || _gameTime - _lastBuzz <= 0.6f
            || _random.NextFloat() >= 0.3f)
        {
            return;
        }

        _lastBuzz = _gameTime;
        var sound = _audio.PlayStatic(_random.NextFloat() < 0.7f ? NullspaceAudio.Buzz : NullspaceAudio.Sparks,
            Filter.Local(),
            ToCoordinates(map, position),
            false,
            NullspaceAudio.Positional(0.22f));
        Unoccluded(sound);
        ApplyReverb(sound);
    }

    private void PlayDeath()
    {
        _audio.PlayGlobal(NullspaceAudio.HonkDistorted, Filter.Local(), false, NullspaceAudio.Global(2.4f));
        _audio.PlayGlobal(NullspaceAudio.Stinger, Filter.Local(), false, NullspaceAudio.Global(1f));
    }
}
