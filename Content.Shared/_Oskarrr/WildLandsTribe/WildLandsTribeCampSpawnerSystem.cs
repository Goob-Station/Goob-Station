// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Robust.Shared.Map;
using Robust.Shared.Network;

namespace Content.Shared._Oskarrr.WildLandsTribe;

public sealed class WildLandsTribeCampSpawnerSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WildLandsTribeCampSpawnerComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<WildLandsTribeCampSpawnerComponent> ent, ref MapInitEvent args)
    {
        if (_net.IsClient)
            return;

        var coords = _xform.GetMoverCoordinates(ent.Owner);
        var i = 0;
        foreach (var proto in ent.Comp.Prototypes)
        {
            var angle = (MathF.PI * 2f / Math.Max(ent.Comp.Prototypes.Count, 1)) * i;
            var offset = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * ent.Comp.Offset;
            Spawn(proto, coords.Offset(offset));
            i++;
        }

        QueueDel(ent.Owner);
    }
}
