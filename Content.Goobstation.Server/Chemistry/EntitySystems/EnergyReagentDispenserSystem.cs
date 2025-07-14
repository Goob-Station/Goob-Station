// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Clyybber <darkmine956@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2023 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 deltanedas <deltanedas@laptop>
// SPDX-FileCopyrightText: 2023 deltanedas <user@zenith>
// SPDX-FileCopyrightText: 2024 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2024 AWF <you@example.com>
// SPDX-FileCopyrightText: 2024 Brandon Li <48413902+aspiringLich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Cojoke <83733158+Cojoke-dot@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 GitHubUser53123 <110841413+GitHubUser53123@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Jake Huxell <JakeHuxell@pm.me>
// SPDX-FileCopyrightText: 2024 Kevin Zheng <kevinz5000@gmail.com>
// SPDX-FileCopyrightText: 2024 Kira Bridgeton <161087999+Verbalase@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Chemistry.Components;
using Content.Goobstation.Shared.Chemistry;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Chemistry.Dispenser;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Containers.ItemSlots;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Power;
using JetBrains.Annotations;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Content.Shared.Labels.Components;
using Content.Server.Power.Components;
using Robust.Shared.Player;
using Robust.Shared.Utility;
using Content.Server.Power.EntitySystems;

namespace Content.Goobstation.Server.Chemistry.EntitySystems
{
    /// <summary>
    /// Contains all the server-side logic for reagent dispensers.
    /// <seealso cref="ReagentDispenserComponent"/>
    /// </summary>
    [UsedImplicitly]
    public sealed class EnergyReagentDispenserSystem : EntitySystem
    {
        [Dependency] private readonly AudioSystem _audioSystem = default!;
        [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
        [Dependency] private readonly SolutionTransferSystem _solutionTransferSystem = default!;
        [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
        [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly OpenableSystem _openable = default!;
        [Dependency] private readonly BatterySystem _battery = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<EnergyReagentDispenserComponent, ComponentStartup>(SubscribeUpdateUiState);
            SubscribeLocalEvent<EnergyReagentDispenserComponent, SolutionContainerChangedEvent>(SubscribeUpdateUiState);
            SubscribeLocalEvent<EnergyReagentDispenserComponent, EntInsertedIntoContainerMessage>(SubscribeUpdateUiState);
            SubscribeLocalEvent<EnergyReagentDispenserComponent, EntRemovedFromContainerMessage>(SubscribeUpdateUiState);
            SubscribeLocalEvent<EnergyReagentDispenserComponent, BoundUIOpenedEvent>(SubscribeUpdateUiState);

            SubscribeLocalEvent<EnergyReagentDispenserComponent, EnergyReagentDispenserSetDispenseAmountMessage>(OnSetDispenseAmountMessage);
            SubscribeLocalEvent<EnergyReagentDispenserComponent, EnergyReagentDispenserDispenseReagentMessage>(OnDispenseReagentMessage);
            SubscribeLocalEvent<EnergyReagentDispenserComponent, EnergyReagentDispenserClearContainerSolutionMessage>(OnClearContainerSolutionMessage);
            SubscribeLocalEvent<EnergyReagentDispenserComponent, PowerChangedEvent>(OnPowerChanged);

            SubscribeLocalEvent<EnergyReagentDispenserComponent, MapInitEvent>(OnMapInit, before: new[]{typeof(ItemSlotsSystem)});
        }

        private void SubscribeUpdateUiState<T>(Entity<EnergyReagentDispenserComponent> ent, ref T ev)
        {
            UpdateUiState(ent);
        }

        private void UpdateUiState(Entity<EnergyReagentDispenserComponent> reagentDispenser)
        {
            var outputContainer = _itemSlotsSystem.GetItemOrNull(reagentDispenser, SharedEnergyReagentDispenser.OutputSlotName);
            var outputContainerInfo = BuildOutputContainerInfo(outputContainer);
            var inventory = GetInventory();

            var batteryCharge = 0f;
            var batteryMaxCharge = 0f;
            if (TryComp<BatteryComponent>(reagentDispenser, out var battery))
            {
                batteryCharge = battery.CurrentCharge;
                batteryMaxCharge = battery.MaxCharge;
            }

            var state = new EnergyReagentDispenserBoundUserInterfaceState(
                outputContainerInfo,
                GetNetEntity(outputContainer),
                inventory,
                reagentDispenser.Comp.DispenseAmount,
                batteryCharge,
                batteryMaxCharge
            );
            _userInterfaceSystem.SetUiState(reagentDispenser.Owner, ReagentDispenserUiKey.Key, state);
        }

        private ContainerInfo? BuildOutputContainerInfo(EntityUid? container)
        {
            if (container is not { Valid: true })
                return null;

            if (_solutionContainerSystem.TryGetFitsInDispenser(container.Value, out _, out var solution))
            {
                return new ContainerInfo(Name(container.Value), solution.Volume, solution.MaxVolume)
                {
                    Reagents = solution.Contents
                };
            }

            return null;
        }

        private List<EnergyReagentInventoryItem> GetInventory()
        {
            var inventory = new List<EnergyReagentInventoryItem>();
            var reagents = _prototypeManager.EnumeratePrototypes<ReagentPrototype>();

            foreach (var reagent in reagents)
            {
                if (reagent.PowerCostPerUnit <= 0)
                    continue;

                inventory.Add(new EnergyReagentInventoryItem(
                    reagent.ID,
                    reagent.LocalizedName,
                    reagent.PowerCostPerUnit,
                    reagent.SubstanceColor
                ));
            }

            inventory.Sort((a, b) => a.ReagentLabel.CompareTo(b.ReagentLabel));
            return inventory;
        }

        private void OnSetDispenseAmountMessage(Entity<EnergyReagentDispenserComponent> reagentDispenser, ref EnergyReagentDispenserSetDispenseAmountMessage message)
        {
            reagentDispenser.Comp.DispenseAmount = message.EnergyReagentDispenserDispenseAmount;
            UpdateUiState(reagentDispenser);
            ClickSound(reagentDispenser);
        }

        private void OnPowerChanged(EntityUid uid, EnergyReagentDispenserComponent component, ref PowerChangedEvent args)
        {
            UpdateUiState((uid, component));
        }

        private float GetPowerCostForReagent(string reagentId, int amount)
        {
            return _prototypeManager.TryIndex<ReagentPrototype>(reagentId, out var reagentProto)
                ? reagentProto.PowerCostPerUnit * amount
                : 1.0f * amount; // Fallback

        }

        private void OnDispenseReagentMessage(Entity<EnergyReagentDispenserComponent> reagentDispenser, ref EnergyReagentDispenserDispenseReagentMessage message)
        {
            var outputContainer = _itemSlotsSystem.GetItemOrNull(reagentDispenser, SharedEnergyReagentDispenser.OutputSlotName);
            if (outputContainer is not { Valid: true } || !_solutionContainerSystem.TryGetFitsInDispenser(outputContainer.Value, out var solution, out _))
                return;

            if (!TryComp<BatteryComponent>(reagentDispenser, out var battery))
                return;

            var amount = (int)reagentDispenser.Comp.DispenseAmount;
            var powerRequired = GetPowerCostForReagent(message.ReagentId, amount);

            if (battery.CurrentCharge < powerRequired)
            {
                _audioSystem.PlayPvs(reagentDispenser.Comp.PowerSound, reagentDispenser, AudioParams.Default.WithVolume(-2f));
                return;
            }


            var sol = new Solution(message.ReagentId, amount);
            if (solution.HasValue && _solutionContainerSystem.TryAddSolution(solution.Value, sol))
            {
                _battery.SetCharge(reagentDispenser.Owner, battery.CurrentCharge - powerRequired);
                ClickSound(reagentDispenser);
                UpdateUiState(reagentDispenser);
            }
        }

        private void OnClearContainerSolutionMessage(Entity<EnergyReagentDispenserComponent> reagentDispenser, ref EnergyReagentDispenserClearContainerSolutionMessage message)
        {
            var outputContainer = _itemSlotsSystem.GetItemOrNull(reagentDispenser, SharedEnergyReagentDispenser.OutputSlotName);
            if (outputContainer is not { Valid: true } || !_solutionContainerSystem.TryGetFitsInDispenser(outputContainer.Value, out var solution, out _))
                return;

            _solutionContainerSystem.RemoveAllSolution(solution.Value);
            UpdateUiState(reagentDispenser);
            ClickSound(reagentDispenser);
        }

        private void ClickSound(Entity<EnergyReagentDispenserComponent> reagentDispenser)
        {
            _audioSystem.PlayPvs(reagentDispenser.Comp.ClickSound, reagentDispenser, AudioParams.Default.WithVolume(-2f));
        }

        private void OnMapInit(Entity<EnergyReagentDispenserComponent> entity, ref MapInitEvent args)
        {
            _itemSlotsSystem.AddItemSlot(entity.Owner, SharedEnergyReagentDispenser.OutputSlotName, entity.Comp.BeakerSlot);
        }
    }
}
