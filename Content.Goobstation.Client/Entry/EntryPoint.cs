// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Client.IoC;
using Robust.Shared.ContentPack;
using Robust.Shared.IoC;

namespace Content.Goobstation.Client.Entry;

public sealed class EntryPoint : GameClient
{
    public override void PreInit()
    {
        base.PreInit();
    }

    public override void Init()
    {
        ContentGoobClientIoC.Register();

        IoCManager.BuildGraph();
        IoCManager.InjectDependencies(this);
    }
}