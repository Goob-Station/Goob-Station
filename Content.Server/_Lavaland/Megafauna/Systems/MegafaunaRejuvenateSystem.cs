// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration.Systems;
using Content.Shared._Lavaland.Megafauna.Components;
using Content.Shared._Lavaland.Megafauna.Events;

namespace Content.Server._Lavaland.Megafauna.Systems;

public sealed class MegafaunaRejuvenateSystem : EntitySystem
{
    [Dependency] private readonly RejuvenateSystem _rejuvenate = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MegafaunaRejuvenateComponent, MegafaunaShutdownEvent>(OnMegafaunaShutdown);
    }

    private void OnMegafaunaShutdown(Entity<MegafaunaRejuvenateComponent> ent, ref MegafaunaShutdownEvent args)
        => _rejuvenate.PerformRejuvenate(ent);
}
