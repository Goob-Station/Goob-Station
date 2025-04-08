// SPDX-FileCopyrightText: 2022 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.Graphics;
using Content.Client.Parallax.Data;

namespace Content.Client.Parallax;

/// <summary>
/// A 'prepared' (i.e. texture loaded and ready to use) parallax layer.
/// </summary>
public struct ParallaxLayerPrepared
{
    /// <summary>
    /// The loaded texture for this layer.
    /// </summary>
    public Texture Texture { get; set; }

    /// <summary>
    /// The configuration for this layer.
    /// </summary>
    public ParallaxLayerConfig Config { get; set; }
}
