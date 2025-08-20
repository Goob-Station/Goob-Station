// SPDX-FileCopyrightText: 2025 Kutosss <162154227+Kutosss@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 ReserveBot <211949879+ReserveBot@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
// Reserve Station edit - Updated TC values in comments

using System;
using Robust.Shared.Serialization;

namespace Content.Shared.Preferences
{
    /// <summary>
    /// Type of uplink device used by the traitor
    /// </summary>
    [Serializable, NetSerializable]
    public enum UplinkPreference : byte
    {
        /// <summary>
        /// Standard PDA uplink (100 TC)
        /// </summary>
        PDA,

        /// <summary>
        /// Implanted uplink (90 TC)
        /// </summary>
        Implant,

        /// <summary>
        /// Radio uplink (110 TC)
        /// </summary>
        Radio,

        /// <summary>
        /// Direct telecrystals (150 TC)
        /// </summary>
        Telecrystals
    }
}
