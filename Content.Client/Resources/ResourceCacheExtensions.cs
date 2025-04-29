// SPDX-FileCopyrightText: 2019 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2019 Silver <Silvertorch5@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 E F R <602406+Efruit@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using JetBrains.Annotations;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Utility;

namespace Content.Client.Resources
{
    [PublicAPI]
    public static class ResourceCacheExtensions
    {
        public static Texture GetTexture(this IResourceCache cache, ResPath path)
        {
            return cache.GetResource<TextureResource>(path);
        }

        public static Texture GetTexture(this IResourceCache cache, string path)
        {
            return GetTexture(cache, new ResPath(path));
        }

        public static Font GetFont(this IResourceCache cache, ResPath path, int size)
        {
            return new VectorFont(cache.GetResource<FontResource>(path), size);
        }

        public static Font GetFont(this IResourceCache cache, string path, int size)
        {
            return cache.GetFont(new ResPath(path), size);
        }

        public static Font GetFont(this IResourceCache cache, ResPath[] path, int size)
        {
            var fs = new Font[path.Length];
            for (var i = 0; i < path.Length; i++)
                fs[i] = new VectorFont(cache.GetResource<FontResource>(path[i]), size);

            return new StackedFont(fs);
        }

        public static Font GetFont(this IResourceCache cache, string[] path, int size)
        {
            var rp = new ResPath[path.Length];
            for (var i = 0; i < path.Length; i++)
                rp[i] = new ResPath(path[i]);

            return cache.GetFont(rp, size);
        }
    }
}