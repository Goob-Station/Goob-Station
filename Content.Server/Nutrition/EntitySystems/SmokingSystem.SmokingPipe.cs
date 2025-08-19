// SPDX-FileCopyrightText: 2022 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2023 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 brainfood1183 <113240905+brainfood1183@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 plykiya <plykiya@protonmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Nutrition.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Interaction;
using Content.Shared.Nutrition.Components;
using Content.Shared.Smoking;
using Content.Shared.Temperature;

namespace Content.Server.Nutrition.EntitySystems
{
    public sealed partial class SmokingSystem
    {
        [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;

        private void InitializePipes()
        {
            SubscribeLocalEvent<SmokingPipeComponent, InteractUsingEvent>(OnPipeInteractUsingEvent);
            SubscribeLocalEvent<SmokingPipeComponent, SmokableSolutionEmptyEvent>(OnPipeSolutionEmptyEvent);
            SubscribeLocalEvent<SmokingPipeComponent, AfterInteractEvent>(OnPipeAfterInteract);
            SubscribeLocalEvent<SmokingPipeComponent, ComponentInit>(OnComponentInit);
        }

        public void OnComponentInit(Entity<SmokingPipeComponent> entity, ref ComponentInit args)
        {
            _itemSlotsSystem.AddItemSlot(entity, SmokingPipeComponent.BowlSlotId, entity.Comp.BowlSlot);
        }

        private void OnPipeInteractUsingEvent(Entity<SmokingPipeComponent> entity, ref InteractUsingEvent args)
        {
            if (args.Handled)
                return;

            if (!EntityManager.TryGetComponent(entity, out SmokableComponent? smokable))
                return;

            if (smokable.State != SmokableState.Unlit)
                return;

            var isHotEvent = new IsHotEvent();
            RaiseLocalEvent(args.Used, isHotEvent, false);

            if (!isHotEvent.IsHot)
                return;

            if (TryTransferReagents(entity, (entity.Owner, smokable)))
                SetSmokableState(entity, SmokableState.Lit, smokable);
            args.Handled = true;
        }

        public void OnPipeAfterInteract(Entity<SmokingPipeComponent> entity, ref AfterInteractEvent args)
        {
            var targetEntity = args.Target;
            if (targetEntity == null ||
                !args.CanReach ||
                !EntityManager.TryGetComponent(entity, out SmokableComponent? smokable) ||
                smokable.State == SmokableState.Lit)
                return;

            var isHotEvent = new IsHotEvent();
            RaiseLocalEvent(targetEntity.Value, isHotEvent, true);

            if (!isHotEvent.IsHot)
                return;

            if (TryTransferReagents(entity, (entity.Owner, smokable)))
                SetSmokableState(entity, SmokableState.Lit, smokable);
            args.Handled = true;
        }

        private void OnPipeSolutionEmptyEvent(Entity<SmokingPipeComponent> entity, ref SmokableSolutionEmptyEvent args)
        {
            _itemSlotsSystem.SetLock(entity, entity.Comp.BowlSlot, false);
            SetSmokableState(entity, SmokableState.Unlit);
        }

        // Convert smokable item into reagents to be smoked
        private bool TryTransferReagents(Entity<SmokingPipeComponent> entity, Entity<SmokableComponent> smokable)
        {
            if (entity.Comp.BowlSlot.Item == null)
                return false;

            EntityUid contents = entity.Comp.BowlSlot.Item.Value;

            if (!TryComp<SolutionContainerManagerComponent>(contents, out var reagents) ||
                !_solutionContainerSystem.TryGetSolution(smokable.Owner, smokable.Comp.Solution, out var pipeSolution, out _))
                return false;

            foreach (var (_, soln) in _solutionContainerSystem.EnumerateSolutions((contents, reagents)))
            {
                var reagentSolution = soln.Comp.Solution;
                _solutionContainerSystem.TryAddSolution(pipeSolution.Value, reagentSolution);
            }

            EntityManager.DeleteEntity(contents);

            _itemSlotsSystem.SetLock(entity.Owner, entity.Comp.BowlSlot, true); //no inserting more until current runs out

            return true;
        }
    }
}