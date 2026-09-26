using System.Numerics;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Graphics.RSI;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Slasher.Overlays;

/// <summary>
/// Adds heart eyes over the users eyes.
/// </summary>
public sealed class HeartEyesOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> Shader = "SlasherHeartEyes";

    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private readonly SharedTransformSystem _xform;
    private readonly SharedStealthSystem _stealth;
    private readonly ShaderInstance _shader;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    public HeartEyesOverlay()
    {
        IoCManager.InjectDependencies(this);

        _xform = _entMan.System<SharedTransformSystem>();
        _stealth = _entMan.System<SharedStealthSystem>();
        _shader = _proto.Index(Shader).InstanceUnique();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (args.Viewport.Eye is not { } eye)
            return;

        var handle = args.WorldHandle;
        var eyeRotation = eye.Rotation;
        var rotationMatrix = Matrix3Helpers.CreateRotation(-eyeRotation);
        var bounds = args.WorldAABB.Enlarged(2f);

        var query = _entMan.AllEntityQueryEnumerator<HeartEyesComponent, SpriteComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var sprite, out var xform))
        {
            if (xform.MapID != args.MapId)
                continue;

            var (worldPos, worldRot) = _xform.GetWorldPositionRotation(xform);
            if (!bounds.Contains(worldPos))
                continue;

            var opacity = sprite.Color.A;
            if (_entMan.TryGetComponent<StealthComponent>(uid, out var stealth))
                opacity *= Math.Clamp(_stealth.GetVisibility(uid, stealth), 0f, 1f);

            var facing = SpriteComponent.Layer.GetDirection(RsiDirectionType.Dir4, (worldRot + eyeRotation).Reduced().FlipPositive());
            if (opacity <= 0.001f
                || !comp.EyeOffsets.TryGetValue(facing, out var eyes)
                || eyes.Count == 0)
                continue;

            handle.SetTransform(Matrix3x2.Multiply(rotationMatrix, Matrix3Helpers.CreateTranslation(worldPos)));

            _shader.SetParameter("baseColor", new Vector3(comp.Color.R, comp.Color.G, comp.Color.B));
            _shader.SetParameter("aberration", comp.ChromaticAberration);
            _shader.SetParameter("shake", comp.Shake);
            _shader.SetParameter("opacity", opacity);

            var half = new Vector2(comp.Size * MathF.Max(MathF.Abs(sprite.Scale.X), MathF.Abs(sprite.Scale.Y)));
            foreach (var offset in eyes)
            {
                var center = sprite.Offset + sprite.Rotation.RotateVec(offset * sprite.Scale);
                _shader.SetParameter("seed", uid.Id * 7.31f % 100f + offset.X * 12.9898f + offset.Y * 78.233f);

                handle.UseShader(_shader);
                handle.DrawTextureRect(Texture.White, new Box2(center - half, center + half));
            }
        }

        handle.UseShader(null);
        handle.SetTransform(Matrix3x2.Identity);
    }
}
