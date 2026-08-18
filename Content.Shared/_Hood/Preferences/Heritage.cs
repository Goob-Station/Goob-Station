// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Hood.Preferences;

/// <summary>
/// Cosmetic identity metadata selected by a player for a humanoid profile.
/// This must not be used to derive appearance, stats, jobs, factions, or equipment.
/// </summary>
public enum Heritage : byte
{
    Unspecified = 0,
    White = 1,
    Black = 2,
    Latino = 3,
    Asian = 4,
    NativeAmerican = 5,
}
