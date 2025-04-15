// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Weapons.SmartGun;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using DrawDepth = Content.Shared.DrawDepth.DrawDepth;

namespace Content.Goobstation.Client.Weapons.LaserPointer;

public sealed class LaserPointerOverlay : Overlay
{
    public override OverlaySpace Space => OverlaySpace.WorldSpaceEntities;
    private readonly IEntityManager _entManager;

    private readonly TransformSystem _transform;

    private readonly ShaderInstance _unshadedShader;

    public LaserPointerOverlay(IEntityManager entManager, IPrototypeManager prototype)
    {
        ZIndex = (int) DrawDepth.Effects;

        _entManager = entManager;

        _transform = entManager.System<TransformSystem>();

        _unshadedShader = prototype.Index<ShaderPrototype>("unshaded").Instance();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;
        var bounds = args.WorldAABB;

        var xformQuery = _entManager.GetEntityQuery<TransformComponent>();
        var query = _entManager.EntityQueryEnumerator<LaserPointerManagerComponent>();
        handle.UseShader(_unshadedShader);
        while (query.MoveNext(out var manager))
        {
            foreach (var (netEnt, data) in manager.Data)
            {
                var start = data.Start;
                var end = data.End;

                var ent = _entManager.GetEntity(netEnt);
                if (xformQuery.TryComp(ent, out var xform))
                {
                    var coords = _transform.GetMapCoordinates(ent, xform);
                    if (coords.MapId != MapId.Nullspace)
                        start = coords.Position;
                }

                var (left, right) = MinMax(start.X, end.X);
                var (bottom, top) = MinMax(start.Y, end.Y);

                if (!bounds.Intersects(new Box2(left, bottom, right, top)))
                    continue;

                handle.DrawLine(start, end, data.Color);
            }
        }

        handle.UseShader(null);
    }

    private static (float min, float max) MinMax(float a, float b)
    {
        return a >= b ? (b, a) : (a, b);
    }
}