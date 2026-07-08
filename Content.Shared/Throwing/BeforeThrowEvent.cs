// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;

namespace Content.Shared.Throwing;

[ByRefEvent]
public struct BeforeThrowEvent
{
    public BeforeThrowEvent(EntityUid itemUid, Vector2 direction, float throwSpeed,  EntityUid playerUid)
    {
        ItemUid = itemUid;
        Direction = direction;
        ThrowSpeed = throwSpeed;
        PlayerUid = playerUid;
    }

    public EntityUid ItemUid { get; set; }
    public Vector2 Direction { get; }
    public float ThrowSpeed { get; set;}
    public EntityUid PlayerUid { get; }

    /// <summary>
    /// Goobstation
    /// Whether it should use GrabThrownSystem to throw
    /// </summary>
    public bool GrabThrow { get; set; }

    public bool Cancelled = false;
}
