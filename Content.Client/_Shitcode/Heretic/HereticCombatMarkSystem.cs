// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Heretic;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;

namespace Content.Client._Shitcode.Heretic;

public sealed partial class HereticCombatMarkSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticCombatMarkComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<HereticCombatMarkComponent, ComponentShutdown>(OnShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // i can't think of a better way to do this. everything else has failed
        // god i hate client server i hate client server i hate client server i hate
        var eqe = EntityQueryEnumerator<HereticCombatMarkComponent>();
        while (eqe.MoveNext(out var uid, out var mark))
        {
            if (!TryComp<SpriteComponent>(uid, out var sprite))
                continue;

            if (!sprite.LayerMapTryGet(0, out var layer))
                continue;

            sprite.LayerSetState(layer, mark.Path.ToLower());
        }
    }

    private void OnStartup(Entity<HereticCombatMarkComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        if (sprite.LayerMapTryGet(0, out var l))
        {
            sprite.LayerSetState(l, ent.Comp.Path.ToLower());
            return;
        }

        var rsi = new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Heretic/combat_marks.rsi"), ent.Comp.Path.ToLower());
        var layer = sprite.AddLayer(rsi);

        sprite.LayerMapSet(0, layer);
        sprite.LayerSetShader(layer, "unshaded");
    }
    private void OnShutdown(Entity<HereticCombatMarkComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        if (!sprite.LayerMapTryGet(0, out var layer))
            return;

        sprite.RemoveLayer(layer);
    }
}