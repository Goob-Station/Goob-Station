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

                var dir = targetCoords.Position - coords.Position;
                var length = dir.Length();

                var time = curTime - (data.CreationTime ?? TimeSpan.Zero);
                if (time < TimeSpan.Zero)
                    time = TimeSpan.Zero;

                var texture = _sprite.GetFrame(data.Sprite, time);
                var textureSize = (Vector2) texture.Size;
                var realY = textureSize.Y / EyeManager.PixelsPerMeter;
                var realX = textureSize.X / EyeManager.PixelsPerMeter;
                var segments = (int) MathF.Ceiling(length / realY);
                if (segments <= 0)
                    continue;

                var ratio = length / (segments * realY);
                var angle = dir.ToWorldAngle();
                var normalized = dir.Normalized();
                var modifiedY = realY * ratio;
                var size = new Vector2(realX, modifiedY);
                var extraSize = Vector2.Zero;

                handle.SetTransform(Matrix3Helpers.CreateTranslation(coords.Position));
                for (var i = 0; i < segments; i++)
                {
                    var tex = texture;
                    Vector2 bottomLeft;
                    Vector2 drawSize;
                    if (i == 0 && data.StartSprite is { } start)
                    {
                        tex = _sprite.GetFrame(start, time);
                        (extraSize, drawSize, bottomLeft) = GetData(tex,
                            size,
                            extraSize,
                            realX,
                            realY,
                            i,
                            normalized);
                    }
                    else if (i == segments - 1 && data.EndSprite is { } end)
                    {
                        tex = _sprite.GetFrame(end, time);
                        (extraSize, drawSize, bottomLeft) = GetData(tex,
                            size,
                            extraSize,
                            realX,
                            realY,
                            i,
                            normalized);
                    }
                    else
                    {
                        (extraSize, drawSize, bottomLeft) = GetData(null,
                            size,
                            extraSize,
                            realX,
                            realY,
                            i,
                            normalized);
                    }

                    var quad = Box2.FromDimensions(bottomLeft, drawSize);
                    var quadRotated = new Box2Rotated(quad, angle + Angle.FromDegrees(180), quad.Center);
                    handle.DrawTextureRect(tex, quadRotated, data.Color);
                }
            }
        }

        handle.UseShader(null);
        handle.SetTransform(Matrix3x2.Identity);
    }

    private (Vector2 extraSize, Vector2 drawSize, Vector2 bottomLeft) GetData(Texture? tex,
        Vector2 size,
        Vector2 extraSize,
        float realX,
        float realY,
        int i,
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

        var dir2 = new Vector2(normalized.X * x / 2f, normalized.Y * y / 2f);
        var pos = new Vector2(-x / 2f, -y / 2f) + dir2;
        var bottomLeft = pos + normalized * newSize.Y * i + normalized * extraSize;
        return (extraSize + newSize - size, newSize, bottomLeft);
    }
}
