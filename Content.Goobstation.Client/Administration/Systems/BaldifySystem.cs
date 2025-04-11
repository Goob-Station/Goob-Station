// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Administration.Components;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Humanoid;

public sealed partial class BaldifySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BaldifyComponent, ComponentStartup>(OnBaldify);
        SubscribeLocalEvent<BaldifyComponent, ComponentShutdown>(OnBaldifyRemoved);
    }

    private void OnBaldify(EntityUid uid, BaldifyComponent component, ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite)
            || !TryComp<HumanoidAppearanceComponent>(uid, out var _))
            return;

        int index = 0;
        foreach (var layer in sprite.AllLayers)
        {
            if (layer.Rsi != null
                && layer.Rsi.Path.ToString().Contains(component.TargetLayer))
            {
                component.TargetIndex = index;
                sprite.LayerSetVisible(index, false);
                return;
            }

            index++;
        }
    }
    private void OnBaldifyRemoved(EntityUid uid, BaldifyComponent component, ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite)
            || !TryComp<HumanoidAppearanceComponent>(uid, out var _)
            || component.TargetIndex == null)
            return;

        sprite.LayerSetVisible(component.TargetIndex.Value, true);
    }
}