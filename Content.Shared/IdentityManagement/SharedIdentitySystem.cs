// SPDX-FileCopyrightText: 2022 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Arendian <137322659+Arendian@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Clothing;
using Content.Shared.IdentityManagement.Components;
using Content.Shared.Inventory;
using Robust.Shared.Containers;

namespace Content.Shared.IdentityManagement;

public abstract class SharedIdentitySystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    private static string SlotName = "identity";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IdentityComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<IdentityBlockerComponent, SeeIdentityAttemptEvent>(OnSeeIdentity);
        SubscribeLocalEvent<IdentityBlockerComponent, InventoryRelayedEvent<SeeIdentityAttemptEvent>>((e, c, ev) => OnSeeIdentity(e, c, ev.Args));
        SubscribeLocalEvent<IdentityBlockerComponent, ItemMaskToggledEvent>(OnMaskToggled);
    }

    private void OnSeeIdentity(EntityUid uid, IdentityBlockerComponent component, SeeIdentityAttemptEvent args)
    {
        if (component.Enabled)
        {
            args.TotalCoverage |= component.Coverage;
            if(args.TotalCoverage == IdentityBlockerCoverage.FULL)
                args.Cancel();
        }
    }

    protected virtual void OnComponentInit(EntityUid uid, IdentityComponent component, ComponentInit args)
    {
        component.IdentityEntitySlot = _container.EnsureContainer<ContainerSlot>(uid, SlotName);
    }

    private void OnMaskToggled(Entity<IdentityBlockerComponent> ent, ref ItemMaskToggledEvent args)
    {
        ent.Comp.Enabled = !args.IsToggled;
    }
}
/// <summary>
///     Gets called whenever an entity changes their identity.
/// </summary>
[ByRefEvent]
public record struct IdentityChangedEvent(EntityUid CharacterEntity, EntityUid IdentityEntity);