// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 PrPleGoo <PrPleGoo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <milonpl.git@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Inventory.Events;
using Content.Shared.Overlays;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;

namespace Content.Client.Overlays;

/// <summary>
/// Adds a health bar overlay.
/// </summary>
public sealed class ShowHealthBarsSystem : EquipmentHudSystem<ShowHealthBarsComponent>
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private EntityHealthBarOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShowHealthBarsComponent, AfterAutoHandleStateEvent>(OnHandleState);

        _overlay = new(EntityManager, _prototype);
    }

    private void OnHandleState(Entity<ShowHealthBarsComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        RefreshOverlay();
    }

    protected override void UpdateInternal(RefreshEquipmentHudEvent<ShowHealthBarsComponent> component)
    {
        base.UpdateInternal(component);

        foreach (var comp in component.Components)
        {
            foreach (var damageContainerId in comp.DamageContainers)
            {
                _overlay.DamageContainers.Add(damageContainerId);
            }

            _overlay.StatusIcon = comp.HealthStatusIcon;
        }

        if (!_overlayMan.HasOverlay<EntityHealthBarOverlay>())
        {
            _overlayMan.AddOverlay(_overlay);
        }
    }

    protected override void DeactivateInternal()
    {
        base.DeactivateInternal();

        _overlay.DamageContainers.Clear();
        _overlayMan.RemoveOverlay(_overlay);
    }
}