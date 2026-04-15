// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2024 faint <46868845+ficcialfaint@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Goobstation.Common.Emag.Events;
using Content.Shared.Access.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Emag.Systems;
using Content.Shared.Lube;
using Content.Shared.Slippery;
using Content.Shared.StepTrigger.Components;
using Content.Shared.StepTrigger.Prototypes;
using Content.Shared.StepTrigger.Systems;
using Robust.Shared.Containers;

namespace Content.Shared.PDA
{
    public abstract class SharedPdaSystem : EntitySystem
    {
        [Dependency] protected readonly ItemSlotsSystem ItemSlotsSystem = default!;
        [Dependency] protected readonly SharedAppearanceSystem Appearance = default!;
        [Dependency] private readonly EmagSystem _emag = default!; // Goobstation - Jestographic
        [Dependency] private readonly StepTriggerSystem _stepTrigger = default!; // Goobstation - Jestographic 

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<PdaComponent, ComponentInit>(OnComponentInit);
            SubscribeLocalEvent<PdaComponent, ComponentRemove>(OnComponentRemove);
            SubscribeLocalEvent<PdaComponent, GotEmaggedEvent>(OnGotEmagged); // Goobstation - Jestographic
            SubscribeLocalEvent<PdaComponent, EmagCleanedEvent>(OnEmagCleaned); // Goobstation - Jestographic

            SubscribeLocalEvent<PdaComponent, EntInsertedIntoContainerMessage>(OnItemInserted);
            SubscribeLocalEvent<PdaComponent, EntRemovedFromContainerMessage>(OnItemRemoved);

            SubscribeLocalEvent<PdaComponent, GetAdditionalAccessEvent>(OnGetAdditionalAccess);
        }
        protected virtual void OnComponentInit(EntityUid uid, PdaComponent pda, ComponentInit args)
        {
            if (pda.IdCard != null)
                pda.IdSlot.StartingItem = pda.IdCard;

            ItemSlotsSystem.AddItemSlot(uid, PdaComponent.PdaIdSlotId, pda.IdSlot);
            ItemSlotsSystem.AddItemSlot(uid, PdaComponent.PdaPenSlotId, pda.PenSlot);
            ItemSlotsSystem.AddItemSlot(uid, PdaComponent.PdaPaiSlotId, pda.PaiSlot);

            UpdatePdaAppearance(uid, pda);
        }

        private void OnComponentRemove(EntityUid uid, PdaComponent pda, ComponentRemove args)
        {
            ItemSlotsSystem.RemoveItemSlot(uid, pda.IdSlot);
            ItemSlotsSystem.RemoveItemSlot(uid, pda.PenSlot);
            ItemSlotsSystem.RemoveItemSlot(uid, pda.PaiSlot);
        }

        protected virtual void OnItemInserted(EntityUid uid, PdaComponent pda, EntInsertedIntoContainerMessage args)
        {
            if (args.Container.ID == PdaComponent.PdaIdSlotId)
                pda.ContainedId = args.Entity;
            //goob addition for pen
            if (args.Container.ID == PdaComponent.PdaPenSlotId)
                pda.ContainedPen = args.Entity;

            UpdatePdaAppearance(uid, pda);
        }

        protected virtual void OnItemRemoved(EntityUid uid, PdaComponent pda, EntRemovedFromContainerMessage args)
        {
            if (args.Container.ID == pda.IdSlot.ID)
                pda.ContainedId = null;
            //goob addition for pen
            if (args.Container.ID == pda.PenSlot.ID)
                pda.ContainedPen = null;

            UpdatePdaAppearance(uid, pda);
        }

        private void OnGetAdditionalAccess(EntityUid uid, PdaComponent component, ref GetAdditionalAccessEvent args)
        {
            if (component.ContainedId is { } id)
                args.Entities.Add(id);
        }

        // Goobstation start
        private void OnGotEmagged(Entity<PdaComponent> ent, ref GotEmaggedEvent args)
        {
            if (!_emag.CompareFlag(args.Type, EmagType.Jestographic))
                return;

            if (_emag.CheckFlag(ent, EmagType.Jestographic))
                return;

            EnsureComp<LubedComponent>(ent.Owner);
            EnsureComp<SlipperyComponent>(ent.Owner);
            var stepTrigger = EnsureComp<StepTriggerComponent>(ent.Owner);
            _stepTrigger.SetTriggerGroup(ent.Owner, ent.Comp.Group, stepTrigger);

            args.Handled = true;
        }

        private void OnEmagCleaned(Entity<PdaComponent> ent, ref EmagCleanedEvent args)
        {
            if (args.Handled)
                return;

            RemCompDeferred<LubedComponent>(ent.Owner);
            RemCompDeferred<SlipperyComponent>(ent.Owner);
            RemCompDeferred<StepTriggerComponent>(ent.Owner);
        }
        // Goobstation end
        
        private void UpdatePdaAppearance(EntityUid uid, PdaComponent pda)
        {
            Appearance.SetData(uid, PdaVisuals.IdCardInserted, pda.ContainedId != null);
            //goob addition for pen
            Appearance.SetData(uid, PdaVisuals.PenInserted, pda.ContainedPen != null);
        }

        public virtual void UpdatePdaUi(EntityUid uid, PdaComponent? pda = null)
        {
            // This does nothing yet while I finish up PDA prediction
            // Overriden by the server
        }
    }
}
