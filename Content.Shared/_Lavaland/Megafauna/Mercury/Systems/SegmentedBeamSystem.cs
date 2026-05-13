using Content.Shared._Lavaland.Megafauna.Mercury.Components;
using Content.Shared._Lavaland.Megafauna.Mercury.Events;
using Robust.Shared.Map;
using Robust.Shared.Network;
using System.Numerics;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Systems;

/// <summary>
/// 
/// </summary>
public sealed class SegmentedBeamSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;

    public void TrySegment(EntityUid uid, SegmentedBeamComponent comp)
    {

    }

}
