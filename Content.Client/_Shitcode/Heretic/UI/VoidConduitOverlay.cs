using System.Numerics;
using Content.Shared._Shitcode.Heretic.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._Shitcode.Heretic.UI;

public sealed class VoidConduitOverlay : Overlay
{
    public override OverlaySpace Space => OverlaySpace.WorldSpaceEntities;

    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private readonly TransformSystem _xform;
    private readonly SpriteSystem _sprite;

    private readonly ShaderInstance _unshadedShader;

    public VoidConduitOverlay()
    {
        IoCManager.InjectDependencies(this);
        ZIndex = (int) Shared.DrawDepth.DrawDepth.FloorEffects;

        _xform = _entMan.System<TransformSystem>();
        _sprite = _entMan.System<SpriteSystem>();

        _unshadedShader = _prototype.Index<ShaderPrototype>("unshaded").Instance();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var eye = args.Viewport.Eye;

        if (eye == null)
            return;

        var handle = args.WorldHandle;

        var xformQuery = _entMan.GetEntityQuery<TransformComponent>();

        handle.UseShader(_unshadedShader);
        var query = _entMan.EntityQueryEnumerator<VoidConduitComponent, TransformComponent>();
        while (query.MoveNext(out _, out var conduit, out var xform))
        {
            var (pos, rot) = _xform.GetWorldPositionRotation(xform, xformQuery);

            var texture = _sprite.GetFrame(conduit.OverlaySprite, _timing.CurTime);

            var rotation = Matrix3Helpers.CreateRotation(rot);
            var translation = Matrix3Helpers.CreateTranslation(pos);
            var matrix = Matrix3x2.Multiply(rotation, translation);
            handle.SetTransform(matrix);

            for (var y = -conduit.Range; y <= conduit.Range; y++)
            {
                for (var x = -conduit.Range; x <= conduit.Range; x++)
                {
                    var neighbor = new Vector2(x, y) - new Vector2(0.5f);
                    handle.DrawTexture(texture, neighbor);
                }
            }
        }

        handle.UseShader(null);
        handle.SetTransform(Matrix3x2.Identity);
    }
}
