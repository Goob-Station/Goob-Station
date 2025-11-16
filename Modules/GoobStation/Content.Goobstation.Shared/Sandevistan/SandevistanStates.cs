// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
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
    Disable = 5, // Sound is not looped to if you want to make Adam Smasher just keep this as the only threshold at like 60-70 and give him high LoadPerInactiveSecond. Or just set LoadPerActiveSecond to 0 if sound is not a problem.
    Death = 6,
}
