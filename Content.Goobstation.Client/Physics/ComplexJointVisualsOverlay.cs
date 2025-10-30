// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Goobstation.Common.Physics;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Physics;

public sealed class ComplexJointVisualsOverlay : Overlay
{
    public override OverlaySpace Space => OverlaySpace.WorldSpaceEntities;

    private readonly IEntityManager _entManager;

    private readonly SpriteSystem _sprite;
    private readonly TransformSystem _transform;
    private readonly IGameTiming _timing;

    private readonly ShaderInstance _unshadedShader;

    public ComplexJointVisualsOverlay(IEntityManager entManager, IPrototypeManager prototype, IGameTiming timing)
    {
        ZIndex = 5;

        _entManager = entManager;

        _timing = timing;

        _sprite = entManager.System<SpriteSystem>();
        _transform = entManager.System<TransformSystem>();

        _unshadedShader = prototype.Index<ShaderPrototype>("unshaded").Instance();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;

        var xformQuery = _entManager.GetEntityQuery<TransformComponent>();
        var query = _entManager.EntityQueryEnumerator<ComplexJointVisualsComponent, TransformComponent>();
        handle.UseShader(_unshadedShader);
        var curTime = _timing.CurTime;
        while (query.MoveNext(out var uid, out var beam, out var xform))
        {
            var coords = _transform.GetMapCoordinates(uid, xform);

            foreach (var (netTarget, data) in beam.Data)
            {
                if (!_entManager.TryGetEntity(netTarget, out var target) ||
                    !xformQuery.TryComp(target.Value, out var targetXforn))
                    continue;

                var targetCoords = _transform.GetMapCoordinates(target.Value, targetXforn);

                if (targetCoords.MapId != coords.MapId)
                    continue;

                var ourPos = coords.Position;

                var dir = targetCoords.Position - ourPos;
                var dirLength = dir.Length();
                var length = dirLength / data.Scale.Y;
                if (length <= 0.01f)
                    continue;

                var time = curTime - (data.CreationTime ?? TimeSpan.Zero);
                if (time < TimeSpan.Zero)
                    time = TimeSpan.Zero;

                var texture = _sprite.GetFrame(data.Sprite, time);
                var textureSize = (Vector2) texture.Size;
                var realY = textureSize.Y / EyeManager.PixelsPerMeter;
                var realX = textureSize.X / EyeManager.PixelsPerMeter;

                var segments = (int) MathF.Ceiling(length / realY);

                if (segments == 0)
                    continue;

                var trueLength = segments * realY;
                if (GetStartOrEndRealY(data.StartSprite) is { } realStartY)
                    trueLength += realStartY - realY;
                if ((data.StartSprite == null || segments > 1) &&
                    GetStartOrEndRealY(data.EndSprite) is { } realEndY)
                    trueLength += realEndY - realY;

                if (trueLength <= 0.01f)
                    continue;

                segments = (int) MathF.Ceiling(trueLength / realY);

                var ratio = length / trueLength;
                var normalized = dir / dirLength;
                var angle = normalized.ToWorldAngle() + Angle.FromDegrees(180);
                var modifiedY = realY * ratio;
                var size = new Vector2(realX, modifiedY);
                var extraLen = 0f;

                var scaleMatrix = Matrix3Helpers.CreateScale(data.Scale);
                var worldMatrix = Matrix3Helpers.CreateTranslation(ourPos);
                var scaledWorld = Matrix3x2.Multiply(scaleMatrix, worldMatrix);
                handle.SetTransform(scaledWorld);
                for (var i = 0; i < segments; i++)
                {
                    Texture? tex = null;

                    if (i == 0 && data.StartSprite is { } start)
                        tex = _sprite.GetFrame(start, time);
                    else if (i == segments - 1 && data.EndSprite is { } end)
                        tex = _sprite.GetFrame(end, time);

                    (extraLen, var drawSize, var pos) =
                        GetData(tex, extraLen, realX, realY, i, size, normalized);

                    var quad = Box2.CenteredAround(pos, drawSize);
                    var quadRotated = new Box2Rotated(quad, angle, pos);
                    handle.DrawTextureRect(tex ?? texture, quadRotated, data.Color);
                }
            }
        }

        handle.UseShader(null);
        handle.SetTransform(Matrix3x2.Identity);
    }

    private float? GetStartOrEndRealY(SpriteSpecifier? sprite)
    {
        if (sprite == null)
            return null;

        return (float) _sprite.Frame0(sprite).Size.Y / EyeManager.PixelsPerMeter;
    }

    private (float extraLen, Vector2 drawSize, Vector2 bottomLeft) GetData(Texture? tex,
        float extraLen,
        float realX,
        float realY,
        int i,
        Vector2 size,
        Vector2 normalized)
    {
        var x = realX;
        var y = realY;
        var newSize = size;

        if (tex != null)
        {
            var s = (Vector2) tex.Size;
            x = s.X / EyeManager.PixelsPerMeter;
            y = s.Y / EyeManager.PixelsPerMeter;
            newSize *= new Vector2(x / realX, y / realY);
        }

        var pos = normalized * (newSize.Y * (0.5f + i) + extraLen);
        return (extraLen + (newSize - size).Y, newSize, pos);
    }
}
