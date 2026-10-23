using System.Linq;
using System.Numerics;
using Content.Client.Graphics;
using Content.Goobstation.Client.VoiceChat;
using Content.Goobstation.Shared.Jiggle;
using Content.Shared.Mobs.Systems;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Jiggle;

public sealed partial class JiggleSystem : EntitySystem
{
    [Dependency] private IEyeManager _eye = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private EntityQuery<SpriteComponent> _spriteQuery = default!;

    private static readonly ProtoId<ShaderPrototype> Shader = "Jiggle";
    public const string PostShaderId = "jiggle";

    private static readonly string[] Before = ContentPostShaderIds.AfterBaseEffects
        .Concat(ContentPostShaderIds.BeforeOutlines)
        .ToArray();

    private static readonly string[] After = { VoiceCanadianSystem.PostShaderId };

    private const float MaxFrame = 0.1f;
    private const float MaxStep = 1f / 120f;
    private const float RestEpsilon = 0.0001f;
    private const float VisibleOffset = 0.5f;
    private const float PhaseSpread = 0.618034f;
    private const float MaxAnimationSpeed = 8f;
    private const float MaxAnimationSpin = 40f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<JiggleComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<JiggleComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<JiggleComponent, BeforePostShaderRenderEvent>(OnBeforeRender);
        SubscribeLocalEvent<JiggleComponent, JiggleEmoteEvent>(OnEmote);
    }

    private void OnStartup(Entity<JiggleComponent> ent, ref ComponentStartup args)
    {
        var visuals = EnsureComp<JiggleVisualsComponent>(ent);
        if (!_spriteQuery.TryComp(ent, out var sprite))
            return;

        visuals.LastSpriteOffset = sprite.Offset;
        visuals.LastSpriteRotation = sprite.Rotation;
    }

    private void OnEmote(Entity<JiggleComponent> ent, ref JiggleEmoteEvent args)
    {
        if (!TryComp<JiggleVisualsComponent>(ent, out var visuals))
            return;

        var now = _timing.RealTime;
        visuals.Pulses.Clear();
        foreach (var pulse in args.Pulses)
        {
            visuals.Pulses.Add((now + TimeSpan.FromSeconds(pulse.Delay), pulse.Impulse));
        }
    }

    private void OnShutdown(Entity<JiggleComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<JiggleVisualsComponent>(ent, out var visuals))
            return;

        SetActive((ent, visuals), false);
        visuals.Shader?.Dispose();
        visuals.Shader = null;

        if (!TerminatingOrDeleted(ent))
            RemComp(ent, visuals);
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var now = _timing.RealTime;
        var time = now.TotalSeconds;
        var eyeRotation = _eye.CurrentEye.Rotation;
        var query = EntityQueryEnumerator<JiggleComponent, JiggleVisualsComponent, PhysicsComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var jiggle, out var visuals, out var physics, out var sprite))
        {
            visuals.Sway = GetSway((uid, jiggle), time);

            var animated = sprite.Offset != visuals.LastSpriteOffset || sprite.Rotation != visuals.LastSpriteRotation;
            if (!animated &&
                visuals.Pulses.Count == 0 &&
                physics.LinearVelocity == Vector2.Zero &&
                visuals.LastVelocity == Vector2.Zero &&
                visuals.Offset.LengthSquared() < RestEpsilon &&
                visuals.Speed.LengthSquared() < RestEpsilon)
            {
                visuals.Offset = Vector2.Zero;
                visuals.Speed = Vector2.Zero;
                SetActive((uid, visuals), MathF.Abs(visuals.Sway) >= VisibleOffset);
                continue;
            }

            var xform = Transform(uid);
            var worldRotation = _transform.GetWorldRotation(xform);
            var velocity = (worldRotation - xform.LocalRotation).RotateVec(physics.LinearVelocity);
            var spriteRotation = sprite.NoRotation ? -eyeRotation : worldRotation;
            velocity += spriteRotation.RotateVec(GetAnimationVelocity(jiggle, visuals, sprite, frameTime));
            ApplyPulses(visuals, spriteRotation + sprite.Rotation, now);

            Simulate(jiggle, visuals, velocity, frameTime);
            SetActive((uid, visuals), visuals.Offset.Length() + MathF.Abs(visuals.Sway) >= VisibleOffset);
        }
    }

    private float GetSway(Entity<JiggleComponent> ent, double time)
    {
        if (ent.Comp.SwayAmplitude == 0f || ent.Comp.SwayPeriod <= 0f || _mobState.IsDead(ent))
            return 0f;

        var phase = ent.Owner.Id * PhaseSpread % 1f;
        var cycle = (time / ent.Comp.SwayPeriod + phase) % 1.0;
        return ent.Comp.SwayAmplitude * MathF.Sin((float) (cycle * Math.Tau));
    }

    private static void ApplyPulses(JiggleVisualsComponent visuals, Angle frame, TimeSpan now)
    {
        for (var i = visuals.Pulses.Count - 1; i >= 0; i--)
        {
            var (time, impulse) = visuals.Pulses[i];
            if (time > now)
                continue;

            visuals.Speed += frame.RotateVec(impulse);
            visuals.Pulses.RemoveAt(i);
        }
    }

    private static Vector2 GetAnimationVelocity(JiggleComponent jiggle, JiggleVisualsComponent visuals, SpriteComponent sprite, float frameTime)
    {
        var offsetDelta = sprite.Offset - visuals.LastSpriteOffset;
        var rotationDelta = (float) Math.IEEERemainder((sprite.Rotation - visuals.LastSpriteRotation).Theta, Math.Tau);
        visuals.LastSpriteOffset = sprite.Offset;
        visuals.LastSpriteRotation = sprite.Rotation;

        if (frameTime <= 0f || jiggle.AnimationScale == 0f)
            return Vector2.Zero;

        var velocity = Vector2.Zero;

        var offsetVelocity = offsetDelta / frameTime;
        if (offsetVelocity.Length() <= MaxAnimationSpeed)
            velocity += offsetVelocity;

        var spin = rotationDelta / frameTime;
        if (MathF.Abs(spin) <= MaxAnimationSpin)
        {
            var up = sprite.Rotation.RotateVec(Vector2.UnitY);
            var radius = jiggle.Height * 0.5f * MathF.Abs(sprite.Scale.Y) / EyeManager.PixelsPerMeter;
            velocity += new Vector2(-up.Y, up.X) * spin * radius;
        }

        return velocity * jiggle.AnimationScale;
    }

    private static void Simulate(JiggleComponent jiggle, JiggleVisualsComponent visuals, Vector2 velocity, float frameTime)
    {
        var kick = (velocity - visuals.LastVelocity) * jiggle.Inertia;
        visuals.LastVelocity = velocity;
        visuals.Speed = ClampLength(visuals.Speed - kick, jiggle.MaxSpeed);

        var rest = ClampLength(-velocity * jiggle.Drag, jiggle.MaxOffset);
        var remaining = MathF.Min(frameTime, MaxFrame);
        while (remaining > 0f)
        {
            var dt = MathF.Min(remaining, MaxStep);
            remaining -= dt;

            var accel = (rest - visuals.Offset) * jiggle.Stiffness - visuals.Speed * jiggle.Damping;
            visuals.Speed = ClampLength(visuals.Speed + accel * dt, jiggle.MaxSpeed);
            visuals.Offset = ClampLength(visuals.Offset + visuals.Speed * dt, jiggle.MaxOffset);
        }
    }

    private void SetActive(Entity<JiggleVisualsComponent> ent, bool active)
    {
        if (ent.Comp.Active == active || !_spriteQuery.TryComp(ent, out var sprite))
            return;

        ent.Comp.Active = active;
        if (!active)
        {
            _sprite.RemovePostShader((ent, sprite), PostShaderId);
            return;
        }

        ent.Comp.Shader ??= _prototype.Index(Shader).InstanceUnique();
        _sprite.SetPostShader((ent, sprite), new SpriteComponent.PostShaderArgs(PostShaderId, ent.Comp.Shader)
        {
            RaiseShaderEvent = true,
            Before = Before,
            After = After,
        });
    }

    private void OnBeforeRender(Entity<JiggleComponent> ent, ref BeforePostShaderRenderEvent args)
    {
        if (args.Id != PostShaderId ||
            args.Viewport.Eye == null ||
            !TryComp<JiggleVisualsComponent>(ent, out var visuals))
            return;

        var sprite = args.Sprite;
        var matrix = args.Viewport.GetWorldToLocalMatrix();
        var pixelsPerMeter = Vector2.TransformNormal(Vector2.UnitY, matrix).Length();
        var eyeRotation = args.Viewport.Eye.Rotation;
        var (worldPosition, worldRotation) = _transform.GetWorldPositionRotation(ent);

        var origin = worldPosition;
        if (sprite.Offset != Vector2.Zero)
        {
            origin += sprite.NoRotation
                ? (-eyeRotation).RotateVec(sprite.Offset)
                : worldRotation.RotateVec(sprite.Offset);
        }

        var local = Vector2.Transform(origin, matrix);
        var screenBounds = matrix.TransformBox(_sprite.CalculateBounds((ent, sprite), worldPosition, worldRotation, eyeRotation));
        var center = (Vector2i) screenBounds.Center;
        var anchor = new Vector2(MathF.Round(local.X) - center.X, center.Y - MathF.Round(local.Y));

        var up = sprite.NoRotation
            ? sprite.Rotation.RotateVec(Vector2.UnitY)
            : ToScreen((sprite.Rotation + worldRotation).RotateVec(Vector2.UnitY), matrix, pixelsPerMeter);
        var right = new Vector2(up.Y, -up.X);

        var offset = ToScreen(visuals.Offset, matrix, pixelsPerMeter);
        var cellSize = Vector2.Abs(sprite.Scale) * pixelsPerMeter / EyeManager.PixelsPerMeter;

        var shader = args.Shader;
        shader.SetParameter("anchor", anchor);
        shader.SetParameter("spriteRight", right);
        shader.SetParameter("spriteUp", up);
        shader.SetParameter("cellSize", cellSize);
        shader.SetParameter("jiggle", new Vector2(Vector2.Dot(offset, right) + visuals.Sway, Vector2.Dot(offset, up)));
        shader.SetParameter("base", ent.Comp.Base);
        shader.SetParameter("height", ent.Comp.Height);
    }

    private static Vector2 ToScreen(Vector2 world, Matrix3x2 matrix, float pixelsPerMeter)
    {
        var local = Vector2.TransformNormal(world, matrix) / pixelsPerMeter;
        return new Vector2(local.X, -local.Y);
    }

    private static Vector2 ClampLength(Vector2 vector, float limit)
    {
        var lengthSquared = vector.LengthSquared();
        if (lengthSquared <= limit * limit)
            return vector;

        return vector * (limit / MathF.Sqrt(lengthSquared));
    }
}
