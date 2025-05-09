// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Toggleable;

// Appearance Data key
[Serializable, NetSerializable]
public enum ToggleableLightVisuals : byte
{
    Enabled,
    Color
}

/// <summary>
///     Generic sprite layer keys.
/// </summary>
[Serializable, NetSerializable]
public enum LightLayers : byte
{
    Light,

    /// <summary>
    ///     Used as a key for generic unshaded layers. Not necessarily related to an entity with an actual light source.
    ///     Use this instead of creating a unique single-purpose "unshaded" enum for every visualizer. 
    /// </summary>
    Unshaded,
}