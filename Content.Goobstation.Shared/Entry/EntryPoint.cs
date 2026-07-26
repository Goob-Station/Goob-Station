// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.IoC;
using Robust.Shared.ContentPack;

namespace Content.Goobstation.Shared.Entry;

public sealed class EntryPoint : GameShared
{
    public override void PreInit()
    {
        SharedGoobContentIoC.Register();

        IoCManager.BuildGraph();
        IoCManager.InjectDependencies(this);
    }
}
