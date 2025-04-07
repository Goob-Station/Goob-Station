// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DoutorWhite <68350815+DoutorWhite@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <milonpl.git@proton.me>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 PrPleGoo <PrPleGoo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Atmos.Rotting;
using Content.Shared.Damage;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Overlays;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Client.Overlays;

/// <summary>
/// Shows a healthy icon on mobs.
/// </summary>
public sealed class ShowHealthIconsSystem : EquipmentHudSystem<ShowHealthIconsComponent>
{
    [Dependency] private readonly IPrototypeManager _prototypeMan = default!;

    [ViewVariables]
    public HashSet<string> DamageContainers = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DamageableComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
        SubscribeLocalEvent<ShowHealthIconsComponent, AfterAutoHandleStateEvent>(OnHandleState);
    }

    protected override void UpdateInternal(RefreshEquipmentHudEvent<ShowHealthIconsComponent> component)
    {
        base.UpdateInternal(component);

        foreach (var damageContainerId in component.Components.SelectMany(x => x.DamageContainers))
        {
            DamageContainers.Add(damageContainerId);
        }
    }

    protected override void DeactivateInternal()
    {
        base.DeactivateInternal();

        DamageContainers.Clear();
    }

    private void OnHandleState(Entity<ShowHealthIconsComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        RefreshOverlay();
    }

    private void OnGetStatusIconsEvent(Entity<DamageableComponent> entity, ref GetStatusIconsEvent args)
    {
        if (!IsActive)
            return;

        var healthIcons = DecideHealthIcons(entity);

        args.StatusIcons.AddRange(healthIcons);
    }

    private IReadOnlyList<HealthIconPrototype> DecideHealthIcons(Entity<DamageableComponent> entity)
    {
        var damageableComponent = entity.Comp;

        if (damageableComponent.DamageContainerID == null ||
            !DamageContainers.Contains(damageableComponent.DamageContainerID))
        {
            return Array.Empty<HealthIconPrototype>();
        }

        var result = new List<HealthIconPrototype>();

        // Here you could check health status, diseases, mind status, etc. and pick a good icon, or multiple depending on whatever.
        if (damageableComponent?.DamageContainerID == "Biological")
        {
            if (TryComp<MobStateComponent>(entity, out var state))
            {
                // Since there is no MobState for a rotting mob, we have to deal with this case first.
                if (HasComp<RottingComponent>(entity) && _prototypeMan.TryIndex(damageableComponent.RottingIcon, out var rottingIcon))
                    result.Add(rottingIcon);
                else if (damageableComponent.HealthIcons.TryGetValue(state.CurrentState, out var value) && _prototypeMan.TryIndex(value, out var icon))
                    result.Add(icon);
            }
        }

        return result;
    }
}
