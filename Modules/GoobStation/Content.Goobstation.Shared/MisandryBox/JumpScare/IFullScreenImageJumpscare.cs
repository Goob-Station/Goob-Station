// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Utility;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Content.Goobstation.Shared.MisandryBox.JumpScare;

// Drag this into common if required by core
public interface IFullScreenImageJumpscare
{
    /// <summary>
    /// Sends a jumpscare to client, session being null implies it's called by client.
    /// </summary>
    public void Jumpscare(SpriteSpecifier.Texture image, ICommonSession? session = null);
}

