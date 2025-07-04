// SPDX-FileCopyrightText: 2022 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Threading;
using System.Threading.Tasks;
using Robust.Client.Graphics;

namespace Content.Client.Parallax.Data
{
    [ImplicitDataDefinitionForInheritors]
    public partial interface IParallaxTextureSource
    {
        /// <summary>
        /// Generates or loads the texture.
        /// Note that this should be cached, but not necessarily *here*.
        /// </summary>
        Task<Texture> GenerateTexture(CancellationToken cancel = default);
    }
}
