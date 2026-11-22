using System.Linq;
using System.Numerics;
using Content.Client.Graphics;
using Content.Goobstation.Client.Jiggle;
using Content.Goobstation.Client.VoiceChat;
using Content.Goobstation.Shared.ModSuits.Modules;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.ModSuits;

public sealed partial class ModSphereVisualsSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private EntityQuery<ModSphereFormComponent> _formQuery = default!;
    [Dependency] private EntityQuery<SpriteComponent> _spriteQuery = default!;

    private static readonly ProtoId<ShaderPrototype> Shader = "ModSphere";
    public const string PostShaderId = "mod-sphere";

    private static readonly string[] Before = ContentPostShaderIds.AfterBaseEffects
        .Concat(ContentPostShaderIds.BeforeOutlines)
        .ToArray();

    private static readonly string[] After = { JiggleSystem.PostShaderId, VoiceCanadianSystem.PostShaderId };

    private const float MorphTime = 0.3f;
    private const float VisualRadius = 10f / EyeManager.PixelsPerMeter;
    private const float SquashTime = 0.18f;
    private const float RollSoundMinSpeed = 0.3f;
    private const float RollSoundReferenceSpeed = 7.8f;

    private static readonly SoundSpecifier RollSound = new SoundPathSpecifier("/Audio/_Goobstation/Items/Modsuit/rolling_ball_loop.ogg");

    private readonly List<EntityUid> _finished = new();

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var forms = EntityQueryEnumerator<ModSphereFormComponent, SpriteComponent>();
        while (forms.MoveNext(out var uid, out _, out var sprite))
        {
            if (HasComp<ModSphereVisualsComponent>(uid))
                continue;

            var visuals = AddComp<ModSphereVisualsComponent>(uid);
            visuals.Shader = _prototype.Index(Shader).InstanceUnique();
            _sprite.SetPostShader((uid, sprite), new SpriteComponent.PostShaderArgs(PostShaderId, visuals.Shader)
            {
                RaiseShaderEvent = true,
                Before = Before,
                After = After,
            });
        }

        _finished.Clear();
        var query = EntityQueryEnumerator<ModSphereVisualsComponent>();
        while (query.MoveNext(out var uid, out var visuals))
        {
            if (_formQuery.TryComp(uid, out var form))
            {
                visuals.PrimaryColor = form.PrimaryColor;
                visuals.DarkColor = form.DarkColor;
                visuals.AccentColor = form.AccentColor;
                visuals.LastImpact = form.LastImpact;
                visuals.ImpactStrength = form.ImpactStrength;
                visuals.ImpactDirection = form.ImpactDirection;
                visuals.Morph = MathF.Min(visuals.Morph + frameTime / MorphTime, 1f);
            }
            else
            {
                visuals.Morph -= frameTime / MorphTime;
                if (visuals.Morph <= 0f)
                {
                    _finished.Add(uid);
                    continue;
                }
            }

            Roll(uid, visuals, frameTime);
            UpdateRollSound(uid, visuals, form != null);
        }

        foreach (var uid in _finished)
        {
            if (!TryComp<ModSphereVisualsComponent>(uid, out var visuals))
                continue;

            if (_spriteQuery.TryComp(uid, out var sprite))
                _sprite.RemovePostShader((uid, sprite), PostShaderId);

            visuals.Shader?.Dispose();
            visuals.Shader = null;
            visuals.RollStream = _audio.Stop(visuals.RollStream);
            RemComp(uid, visuals);
        }
    }

    private void Roll(EntityUid uid, ModSphereVisualsComponent visuals, float frameTime)
    {
        var velocity = _physics.GetMapLinearVelocity(uid);
        var speed = velocity.Length();
        if (speed < 0.01f)
            return;

        var axis = new Vector3(-velocity.Y / speed, velocity.X / speed, 0f);
        var angle = speed / VisualRadius * frameTime;
        var delta = Quaternion.CreateFromAxisAngle(axis, angle);
        visuals.Orientation = Quaternion.Normalize(delta * visuals.Orientation);
    }

    private void UpdateRollSound(EntityUid uid, ModSphereVisualsComponent visuals, bool rolling)
    {
        var speed = rolling ? _physics.GetMapLinearVelocity(uid).Length() : 0f;
        if (speed < RollSoundMinSpeed)
        {
            visuals.RollStream = _audio.Stop(visuals.RollStream);
            return;
        }

        if (!TryComp<AudioComponent>(visuals.RollStream, out var audio))
        {
            var stream = _audio.PlayEntity(RollSound, Filter.Local(), uid, false, AudioParams.Default.WithLoop(true));
            if (stream == null)
                return;

            visuals.RollStream = stream.Value.Entity;
            audio = stream.Value.Component;
        }

        var intensity = Math.Clamp(speed / RollSoundReferenceSpeed, 0f, 1f);
        _audio.SetVolume(visuals.RollStream, MathHelper.Lerp(-18f, -6f, intensity), audio);
    }

    [SubscribeLocalEvent]
    private void OnBeforeRender(Entity<ModSphereVisualsComponent> ent, ref BeforePostShaderRenderEvent args)
    {
        if (args.Id != PostShaderId || args.Viewport.Eye == null)
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
        var cellSize = Vector2.Abs(sprite.Scale) * pixelsPerMeter / EyeManager.PixelsPerMeter;

        var spriteAngle = (sprite.NoRotation ? -eyeRotation : worldRotation) + sprite.Rotation;
        var worldRight = spriteAngle.RotateVec(Vector2.UnitX);
        var worldUp = spriteAngle.RotateVec(Vector2.UnitY);

        var shader = args.Shader;
        shader.SetParameter("anchor", anchor);
        shader.SetParameter("spriteRight", right);
        shader.SetParameter("spriteUp", up);
        shader.SetParameter("cellSize", cellSize);
        shader.SetParameter("rowX", BodyRow(ent.Comp.Orientation, Vector3.UnitX, worldRight, worldUp));
        shader.SetParameter("rowY", BodyRow(ent.Comp.Orientation, Vector3.UnitY, worldRight, worldUp));
        shader.SetParameter("rowZ", BodyRow(ent.Comp.Orientation, Vector3.UnitZ, worldRight, worldUp));
        shader.SetParameter("primaryColor", ent.Comp.PrimaryColor);
        shader.SetParameter("darkColor", ent.Comp.DarkColor);
        shader.SetParameter("accentColor", ent.Comp.AccentColor);
        shader.SetParameter("morph", ent.Comp.Morph);
        shader.SetParameter("squash", GetSquash(ent.Comp));
        shader.SetParameter("squashDir", new Vector2(
            Vector2.Dot(ent.Comp.ImpactDirection, worldRight),
            Vector2.Dot(ent.Comp.ImpactDirection, worldUp)));
    }

    private static Vector3 BodyRow(Quaternion orientation, Vector3 axis, Vector2 worldRight, Vector2 worldUp)
    {
        var body = Vector3.Transform(axis, orientation);
        var flat = new Vector2(body.X, body.Y);
        return new Vector3(Vector2.Dot(worldRight, flat), Vector2.Dot(worldUp, flat), body.Z);
    }

    private float GetSquash(ModSphereVisualsComponent visuals)
    {
        var elapsed = (float) (_timing.CurTime - visuals.LastImpact).TotalSeconds;
        if (elapsed < 0f || elapsed > SquashTime)
            return 0f;

        return visuals.ImpactStrength * (1f - elapsed / SquashTime);
    }

    private static Vector2 ToScreen(Vector2 world, Matrix3x2 matrix, float pixelsPerMeter)
    {
        var local = Vector2.TransformNormal(world, matrix) / pixelsPerMeter;
        return new Vector2(local.X, -local.Y);
    }
}
