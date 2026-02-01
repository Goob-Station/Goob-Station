using System.Numerics;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Shaders;

public sealed class DrawingHandleMultiShader(
    Texture white,
    DrawingHandleWorld handle) : DrawingHandleWorld(white)
{
    public Matrix3x2 InvMatrix = Matrix3x2.Identity;

    public override void SetTransform(in Matrix3x2 matrix)
    {
        handle.SetTransform(Matrix3x2.Multiply(matrix, InvMatrix));
    }

    public override Matrix3x2 GetTransform()
    {
        return handle.GetTransform();
    }

    public override void UseShader(ShaderInstance? shader)
    {
        handle.UseShader(shader);
    }

    public override ShaderInstance? GetShader()
    {
        return handle.GetShader();
    }

    public override void DrawPrimitives(DrawPrimitiveTopology primitiveTopology,
        Texture texture,
        ReadOnlySpan<DrawVertexUV2DColor> vertices)
    {
        handle.DrawPrimitives(primitiveTopology, texture, vertices);
    }

    public override void DrawPrimitives(DrawPrimitiveTopology primitiveTopology,
        Texture texture,
        ReadOnlySpan<ushort> indices,
        ReadOnlySpan<DrawVertexUV2DColor> vertices)
    {
        handle.DrawPrimitives(primitiveTopology, texture, indices, vertices);
    }

    public override void DrawCircle(Vector2 position, float radius, Color color, bool filled = true)
    {
        handle.DrawCircle(position, radius, color, filled);
    }

    public override void DrawLine(Vector2 from, Vector2 to, Color color)
    {
        handle.DrawLine(from, to, color);
    }

    public override void RenderInRenderTarget(IRenderTarget t, Action a, Color? clearColor)
    {
        handle.RenderInRenderTarget(t, a, clearColor);
    }

    public override void DrawRect(Box2 rect, Color color, bool filled = true)
    {
        handle.DrawRect(rect, color, filled);
    }

    public override void DrawRect(in Box2Rotated rect, Color color, bool filled = true)
    {
        handle.DrawRect(in rect, color, filled);
    }

    public override void DrawTextureRectRegion(Texture texture,
        Box2 quad,
        Color? modulate = null,
        UIBox2? subRegion = null)
    {
        handle.DrawTextureRectRegion(texture, quad, modulate, subRegion);
    }

    public override void DrawTextureRectRegion(Texture texture,
        in Box2Rotated quad,
        Color? modulate = null,
        UIBox2? subRegion = null)
    {
        handle.DrawTextureRectRegion(texture, in quad, modulate, subRegion);
    }
}
