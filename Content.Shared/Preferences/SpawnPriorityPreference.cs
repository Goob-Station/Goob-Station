// SPDX-FileCopyrightText: 2024 Krunklehorn <42424291+Krunklehorn@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT
namespace Content.Shared.Preferences
{
    /// <summary>
    /// The spawn priority preference for a profile. Stored in database!
    /// </summary>
    public enum SpawnPriorityPreference
    {
        ///////////////////////
        /// DO NOT TOUCH!!! ///
        ///////////////////////
        None = 0,
        Arrivals = 1,
        Cryosleep = 2,
    }
}