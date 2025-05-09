// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoidaBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Module;

namespace Content.Goidastation.Shared.Module;

public sealed class GoidaModPack : ModulePack
{
    public override string PackName => "Goidastation";

    public override IReadOnlySet<RequiredAssembly> RequiredAssemblies { get; } = new HashSet<RequiredAssembly>
    {
        RequiredAssembly.Client("Content.Goidastation.Client"),
        RequiredAssembly.Client("Content.Goidastation.UIKit"),
        RequiredAssembly.Server("Content.Goidastation.Server"),
        RequiredAssembly.Shared("Content.Goidastation.Maths"),
        RequiredAssembly.Shared("Content.Goidastation.Common"),
    };
}
