// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <154002422+LuciferEOS@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Goobstation.Common.Gun.Events;

/// <summary>
/// Raised when entity shoots a gun.
/// </summary>
[Serializable, NetSerializable]
public sealed class UserShotAmmoEvent : EntityEventArgs
{
    public List<NetEntity> FiredProjectiles { get; }
    public NetEntity Gun { get; }

    public UserShotAmmoEvent(List<NetEntity> firedProjectiles, NetEntity gun)
    {
        FiredProjectiles = firedProjectiles;
        Gun = gun;
    }
}
