// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.Graphics;

namespace Content.Client.Explosion;

[RegisterComponent]
public sealed partial class ExplosionVisualsTexturesComponent : Component
{
    /// <summary>
    ///     Uid of the client-side point light entity for this explosion.
    /// </summary>
    public EntityUid LightEntity;

    /// <summary>
    ///     How intense an explosion needs to be at a given tile in order to progress to the next fire-intensity RSI state. See also <see cref="FireFrames"/>
    /// </summary>
    public float IntensityPerState;

    /// <summary>
    ///     The textures used for the explosion fire effect. Each fire-state is associated with an explosion
    ///     intensity range, and each stat itself has several textures.
    /// </summary>
    public List<Texture[]> FireFrames = new();

    public Color? FireColor;
}