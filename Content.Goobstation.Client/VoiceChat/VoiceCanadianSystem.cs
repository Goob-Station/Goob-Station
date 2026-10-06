using System.Numerics;
using Content.Client.Graphics;
using Content.Goobstation.Shared.VoiceChat;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.VoiceChat;

public sealed class VoiceCanadianSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly VoiceChatSystem _voice = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private static readonly ProtoId<ShaderPrototype> Shader = "VoiceCanadian";
    private const string PostShaderId = "voice-canadian";
    private static readonly float[] HingeTargets = { -1f, 0f, 1f };

    private const float Pixel = 1f / EyeManager.PixelsPerMeter;
    private const float MaxRotation = 0.2f;
    private const float OpenThreshold = 0.02f;
    private const float ClosedLevel = 0.15f;
    private const float ReopenLevel = 0.3f;

    private readonly Dictionary<EntityUid, FlapState> _flapping = new();
    private readonly Dictionary<NetEntity, float> _levels = new();
    private readonly List<EntityUid> _stopped = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoiceChatCanadianComponent, BeforePostShaderRenderEvent>(OnBeforeRender);
        SubscribeLocalEvent<VoiceChatCanadianComponent, ComponentShutdown>(OnShutdown);
    }

    public override void Shutdown()
    {
        base.Shutdown();

        foreach (var (uid, state) in _flapping)
        {
            if (TryComp<SpriteComponent>(uid, out var sprite))
                Release(sprite);
        }

        _flapping.Clear();
    }

    public bool ReplacesIndicator(EntityUid uid)
    {
        return HasComp<VoiceChatCanadianComponent>(uid) &&
               TryComp<SpriteComponent>(uid, out var sprite) &&
               CanFlap(uid, sprite);
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        _levels.Clear();
        foreach (var stream in _voice.Streams.Values)
        {
            if (stream.Route != VoiceRoute.Direct || stream.IsMix || _voice.IsSelf(stream.Speaker))
                continue;

            var level = stream.Levels.Overall * stream.Activity;
            if (level > _levels.GetValueOrDefault(stream.Source))
                _levels[stream.Source] = level;
        }

        var now = _timing.RealTime;
        var local = _player.LocalEntity;
        var query = EntityQueryEnumerator<VoiceChatCanadianComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var canadian, out var sprite))
        {
            var level = uid == local
                ? _voice.Self.Levels.Overall * _voice.Self.Activity
                : _levels.GetValueOrDefault(GetNetEntity(uid));

            var open = CanFlap(uid, sprite)
                ? Math.Clamp(level * canadian.Sensitivity, 0f, 1f)
                : 0f;

            UpdateFlap(uid, sprite, canadian, open, now, frameTime);
        }

        _stopped.Clear();
        foreach (var uid in _flapping.Keys)
        {
            if (!HasComp<VoiceChatCanadianComponent>(uid))
                _stopped.Add(uid);
        }

        foreach (var uid in _stopped)
        {
            if (_flapping.Remove(uid) && TryComp<SpriteComponent>(uid, out var sprite))
                Release(sprite);
        }
    }

    private bool CanFlap(EntityUid uid, SpriteComponent sprite)
    {
        var rotation = Math.Abs(sprite.Rotation.Reduced().Theta);
        if (!sprite.NoRotation || Math.Min(rotation, Math.Tau - rotation) > MaxRotation)
            return false;

        return !_sprite.HasPostShader(sprite, ContentPostShaderIds.Stealth);
    }

    private void UpdateFlap(EntityUid uid, SpriteComponent sprite, VoiceChatCanadianComponent canadian, float open, TimeSpan now, float frameTime)
    {
        if (open < OpenThreshold)
        {
            if (_flapping.Remove(uid))
                Release(sprite);

            return;
        }

        if (!_flapping.TryGetValue(uid, out var state))
        {
            state = new FlapState(_prototype.Index(Shader).InstanceUnique());
            _sprite.SetPostShader(sprite, new SpriteComponent.PostShaderArgs(PostShaderId, state.Shader)
            {
                RaiseShaderEvent = true,
                Before = ContentPostShaderIds.BeforeOutlines,
            });
            state.Target = _random.Pick(HingeTargets);
            state.Hinge = state.Target;
            state.NextSwitch = now + NextPivotTime(canadian);
            _flapping[uid] = state;
        }

        if (open < ClosedLevel)
        {
            state.Closed = true;
        }
        else if (state.Closed && open > ReopenLevel || now >= state.NextSwitch)
        {
            state.Closed = false;
            state.Target = PickTarget(state.Target);
            state.NextSwitch = now + NextPivotTime(canadian);
        }

        var smoothing = canadian.PivotSmoothing > 0f ? 1f - MathF.Exp(-frameTime / canadian.PivotSmoothing) : 1f;
        state.Hinge += (state.Target - state.Hinge) * smoothing;
        state.Open = open;
    }

    private void OnBeforeRender(EntityUid uid, VoiceChatCanadianComponent component, ref BeforePostShaderRenderEvent args)
    {
        if (args.Id != PostShaderId || !_flapping.TryGetValue(uid, out var state))
            return;

        var matrix = args.Viewport.GetWorldToLocalMatrix();
        var pixelsPerMeter = (Vector2.Transform(Vector2.UnitY, matrix) - Vector2.Transform(Vector2.Zero, matrix)).Length();
        var bounds = _sprite.GetLocalBounds((uid, args.Sprite));

        var hinge = state.Hinge;
        var straightness = 1f - MathF.Abs(hinge);
        var liftPixels = component.HingeLift + (component.MaxLift - component.HingeLift) * straightness;

        var mouthY = (component.MouthHeight * Pixel - bounds.Center.Y) * pixelsPerMeter;
        var pivotX = (hinge * component.HingeOffset * Pixel - bounds.Center.X) * pixelsPerMeter;
        var angle = -hinge * MathHelper.DegreesToRadians(component.MaxAngle) * state.Open;
        var lift = liftPixels * Pixel * pixelsPerMeter * state.Open;

        state.Shader.SetParameter("pivot", new Vector2(pivotX, mouthY));
        state.Shader.SetParameter("mouthY", mouthY);
        state.Shader.SetParameter("angle", angle);
        state.Shader.SetParameter("lift", lift);
    }

    private void OnShutdown(Entity<VoiceChatCanadianComponent> ent, ref ComponentShutdown args)
    {
        if (_flapping.Remove(ent.Owner) && TryComp<SpriteComponent>(ent, out var sprite))
            Release(sprite);
    }

    private float PickTarget(float current)
    {
        var target = _random.Pick(HingeTargets);
        return MathHelper.CloseTo(target, current) ? _random.Pick(HingeTargets) : target;
    }

    private TimeSpan NextPivotTime(VoiceChatCanadianComponent canadian)
    {
        var min = MathF.Max(0.05f, canadian.MinPivotTime);
        var max = MathF.Max(min, canadian.MaxPivotTime);
        return TimeSpan.FromSeconds(_random.NextFloat(min, max));
    }

    private void Release(SpriteComponent sprite)
    {
        _sprite.RemovePostShader(sprite, PostShaderId);
    }

    private sealed class FlapState(ShaderInstance shader)
    {
        public readonly ShaderInstance Shader = shader;
        public float Open;
        public bool Closed;
        public float Hinge;
        public float Target;
        public TimeSpan NextSwitch;
    }
}
