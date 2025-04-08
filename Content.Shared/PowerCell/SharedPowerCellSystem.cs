// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Vasilis <vasilis@pikachu.systems>
// SPDX-FileCopyrightText: 2024 12rabbits <53499656+12rabbits@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Alzore <140123969+Blackern5000@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ArtisticRoomba <145879011+ArtisticRoomba@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Brandon Hu <103440971+Brandon-Huu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Dimastra <65184747+Dimastra@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Dimastra <dimastra@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Eoin Mcloughlin <helloworld@eoinrul.es>
// SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 JIPDawg <51352440+JIPDawg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 JIPDawg <JIPDawg93@gmail.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2024 Moomoobeef <62638182+Moomoobeef@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 PopGamer46 <yt1popgamer@gmail.com>
// SPDX-FileCopyrightText: 2024 PursuitInAshes <pursuitinashes@gmail.com>
// SPDX-FileCopyrightText: 2024 QueerNB <176353696+QueerNB@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Saphire Lattice <lattice@saphi.re>
// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Spessmann <156740760+Spessmann@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Thomas <87614336+Aeshus@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 eoineoineoin <github@eoinrul.es>
// SPDX-FileCopyrightText: 2024 github-actions[bot] <41898282+github-actions[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 stellar-novas <stellar_novas@riseup.net>
// SPDX-FileCopyrightText: 2024 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Containers.ItemSlots;
using Content.Shared.PowerCell.Components;
using Content.Shared.Rejuvenate;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Shared.PowerCell;

public abstract class SharedPowerCellSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PowerCellDrawComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<PowerCellSlotComponent, RejuvenateEvent>(OnRejuvenate);
        SubscribeLocalEvent<PowerCellSlotComponent, EntInsertedIntoContainerMessage>(OnCellInserted);
        SubscribeLocalEvent<PowerCellSlotComponent, EntRemovedFromContainerMessage>(OnCellRemoved);
        SubscribeLocalEvent<PowerCellSlotComponent, ContainerIsInsertingAttemptEvent>(OnCellInsertAttempt);
    }

    private void OnMapInit(Entity<PowerCellDrawComponent> ent, ref MapInitEvent args)
    {
        QueueUpdate((ent, ent.Comp));
    }

    private void OnRejuvenate(EntityUid uid, PowerCellSlotComponent component, RejuvenateEvent args)
    {
        if (!_itemSlots.TryGetSlot(uid, component.CellSlotId, out var itemSlot) || !itemSlot.Item.HasValue)
            return;

        // charge entity batteries and remove booby traps.
        RaiseLocalEvent(itemSlot.Item.Value, args);
    }

    private void OnCellInsertAttempt(EntityUid uid, PowerCellSlotComponent component, ContainerIsInsertingAttemptEvent args)
    {
        if (!component.Initialized)
            return;

        if (args.Container.ID != component.CellSlotId)
            return;

        if (!HasComp<PowerCellComponent>(args.EntityUid))
        {
            args.Cancel();
        }
    }

    private void OnCellInserted(EntityUid uid, PowerCellSlotComponent component, EntInsertedIntoContainerMessage args)
    {
        if (!component.Initialized)
            return;

        if (args.Container.ID != component.CellSlotId)
            return;
        _appearance.SetData(uid, PowerCellSlotVisuals.Enabled, true);
        RaiseLocalEvent(uid, new PowerCellChangedEvent(false), false);
    }

    protected virtual void OnCellRemoved(EntityUid uid, PowerCellSlotComponent component, EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != component.CellSlotId)
            return;
        _appearance.SetData(uid, PowerCellSlotVisuals.Enabled, false);
        RaiseLocalEvent(uid, new PowerCellChangedEvent(true), false);
    }

    /// <summary>
    /// Makes the draw logic update in the next tick.
    /// </summary>
    public void QueueUpdate(Entity<PowerCellDrawComponent?> ent)
    {
        if (Resolve(ent, ref ent.Comp))
            ent.Comp.NextUpdateTime = Timing.CurTime;
    }

    public void SetDrawEnabled(Entity<PowerCellDrawComponent?> ent, bool enabled)
    {
        if (!Resolve(ent, ref ent.Comp, false) || ent.Comp.Enabled == enabled)
            return;

        ent.Comp.Enabled = enabled;
        Dirty(ent, ent.Comp);
    }

    /// <summary>
    /// Returns whether the entity has a slotted battery and <see cref="PowerCellDrawComponent.UseRate"/> charge.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="battery"></param>
    /// <param name="cell"></param>
    /// <param name="user">Popup to this user with the relevant detail if specified.</param>
    public abstract bool HasActivatableCharge(
        EntityUid uid,
        PowerCellDrawComponent? battery = null,
        PowerCellSlotComponent? cell = null,
        EntityUid? user = null);

    /// <summary>
    /// Whether the power cell has any power at all for the draw rate.
    /// </summary>
    public abstract bool HasDrawCharge(
        EntityUid uid,
        PowerCellDrawComponent? battery = null,
        PowerCellSlotComponent? cell = null,
        EntityUid? user = null);
}