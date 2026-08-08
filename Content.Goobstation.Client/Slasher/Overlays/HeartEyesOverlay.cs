using System.Numerics;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Slasher.Overlays;

/// <summary>
/// Draws heart eyes on top of the user.
/// </summary>
public sealed class HeartEyesOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> Shader = "SlasherHeartEyes";

    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private readonly ShaderInstance _shader;
    private SharedTransformSystem? _xform;
    private SharedStealthSystem? _stealth;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    public HeartEyesOverlay()
    {
        IoCManager.InjectDependencies(this);
        _shader = _proto.Index(Shader).InstanceUnique();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (args.Viewport.Eye == null)
            return;

        if (_xform == null
            && !_entMan.TrySystem(out _xform))
            return;

        if (_stealth == null
            && !_entMan.TrySystem(out _stealth))
            return;

        var handle = args.WorldHandle;
        var eyeRot = args.Viewport.Eye.Rotation;
        var rotationMatrix = Matrix3Helpers.CreateRotation(-eyeRot);
        var bounds = args.WorldAABB.Enlarged(2f);

        var query = _entMan.AllEntityQueryEnumerator<HeartEyesComponent, SpriteComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var sprite, out var xform))
        {
            if (xform.MapID != args.MapId)
                continue;

            var worldPos = _xform.GetWorldPosition(xform);
            if (!bounds.Contains(worldPos))
                continue;

            var opacity = sprite.Color.A;
            if (_entMan.TryGetComponent<StealthComponent>(uid, out var stealth))
                opacity *= Math.Clamp(_stealth.GetVisibility(uid, stealth), 0f, 1f);
            if (opacity <= 0.001f)
                continue;

            var dir = EyeOverlayHelpers.GetSpriteDirection(_xform.GetWorldRotation(xform) + eyeRot);
            if (!comp.EyeOffsets.TryGetValue(dir, out var eyes) || eyes.Count == 0)
                continue;

            var worldMatrix = Matrix3Helpers.CreateTranslation(worldPos);
            handle.SetTransform(Matrix3x2.Multiply(rotationMatrix, worldMatrix));

            _shader.SetParameter("baseColor", new Vector3(comp.Color.R, comp.Color.G, comp.Color.B));
            _shader.SetParameter("aberration", comp.ChromaticAberration);
            _shader.SetParameter("shake", comp.Shake);
            _shader.SetParameter("opacity", opacity);

            var scale = sprite.Scale;
            var spriteRot = sprite.Rotation;
            var half = comp.Size * MathF.Max(MathF.Abs(scale.X), MathF.Abs(scale.Y));
            foreach (var eye in eyes)
            {
                var pos = sprite.Offset + spriteRot.RotateVec(eye * scale);
                _shader.SetParameter("seed", eye.X * 12.9898f + eye.Y * 78.233f);

                handle.UseShader(_shader);
                handle.DrawTextureRect(Texture.White, new Box2(pos - new Vector2(half), pos + new Vector2(half)));
                handle.UseShader(null);
            }
        }

        handle.SetTransform(Matrix3x2.Identity);
    }
}
