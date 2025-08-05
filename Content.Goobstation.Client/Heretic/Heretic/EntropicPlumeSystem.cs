// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Heretic.Components;
using Content.Goobstation.Shared.Heretic.Systems;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Heretic.Heretic;

public sealed class EntropicPlumeSystem : SharedEntropicPlumeSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EntropicPlumeAffectedComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<EntropicPlumeAffectedComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnShutdown(Entity<EntropicPlumeAffectedComponent> ent, ref ComponentShutdown args)
    {
        var (uid, _) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite)
            || !sprite.LayerMapTryGet(EntropicPlumeKey.Key, out var layer)) // todo: fix this slop
            return;

        sprite.RemoveLayer(layer);
    }

    private void OnStartup(Entity<EntropicPlumeAffectedComponent> ent, ref ComponentStartup args)
    {
        var (uid, comp) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (sprite.LayerMapTryGet(EntropicPlumeKey.Key, out _))
            return;

        var layer = sprite.AddLayer(comp.Sprite);
        sprite.LayerMapSet(EntropicPlumeKey.Key, layer);
    }
}
