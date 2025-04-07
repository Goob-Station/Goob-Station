// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.IoC;
using Robust.Shared.ContentPack;
using Robust.Shared.IoC;

namespace Content.Goobstation.Common.Entry;

// EntryPoint is marked as GameShared for module registration purposes.
public sealed class EntryPoint : GameShared
{
    public override void PreInit()
    {
        IoCManager.InjectDependencies(this);
        CommonGoobContentIoC.Register();
    }
}