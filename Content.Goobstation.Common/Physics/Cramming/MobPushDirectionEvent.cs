using System.Numerics;
using Robust.Shared.GameObjects;

namespace Content.Goobstation.Common.Physics.Cramming;

/// <summary>
/// Exists to track pressure buildup when multiple mobs for cramming
/// </summary>
[ByRefEvent]
public readonly record struct MobPushDirectionEvent
{
	public EntityUid Mob { get; init; }

	public Vector2 Direction { get; init; }
}
