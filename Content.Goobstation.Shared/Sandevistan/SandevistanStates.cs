// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Sandevistan;

[Serializable, NetSerializable]
public enum SandevistanState : byte
{
    Warning = 0,
    Shaking = 1,
    Stamina = 2,
    Damage = 3,
    Knockdown = 4,
    Disable = 5,
    Death = 6,
}
