// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Xenobiology
{

    //This enum will be expanded as Xenobio increases in scope, Shaders etc...
    [Serializable, NetSerializable]
    public enum XenoSlimeVisuals
    {
        Color,
        Shader,
    }
}
