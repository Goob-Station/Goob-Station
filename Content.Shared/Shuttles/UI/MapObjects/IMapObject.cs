// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Shuttles.UI.MapObjects;

/// <summary>
/// Abstract map object representing a grid, beacon etc for use on the map screen.
/// </summary>
public interface IMapObject
{
    string Name { get; }

    /// <summary>
    /// Should we hide the button from being shown (AKA just draw it).
    /// </summary>
    bool HideButton { get; }
}