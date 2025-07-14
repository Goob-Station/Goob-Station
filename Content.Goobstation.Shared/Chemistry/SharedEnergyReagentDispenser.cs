// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2024 AWF <you@example.com>
// SPDX-FileCopyrightText: 2024 Brandon Li <48413902+aspiringLich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 GitHubUser53123 <110841413+GitHubUser53123@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kevin Zheng <kevinz5000@gmail.com>
// SPDX-FileCopyrightText: 2024 Kira Bridgeton <161087999+Verbalase@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Serialization;
using Content.Shared.Chemistry;

namespace Content.Goobstation.Shared.Chemistry
{
    /// <summary>
    /// This class holds constants that are shared between client and server.
    /// </summary>
    public sealed class SharedEnergyReagentDispenser
    {
        public const string OutputSlotName = "beakerSlot";
    }

    [Serializable, NetSerializable]
    public sealed class EnergyReagentDispenserSetDispenseAmountMessage : BoundUserInterfaceMessage
    {
        public readonly EnergyReagentDispenserDispenseAmount EnergyReagentDispenserDispenseAmount;

        public EnergyReagentDispenserSetDispenseAmountMessage(EnergyReagentDispenserDispenseAmount amount)
        {
            EnergyReagentDispenserDispenseAmount = amount;
        }

        /// <summary>
        ///     Create a new instance from interpreting a String as an integer,
        ///     throwing an exception if it is unable to parse.
        /// </summary>
        public EnergyReagentDispenserSetDispenseAmountMessage(String s)
        {
            switch (s)
            {
                case "1":
                    EnergyReagentDispenserDispenseAmount = EnergyReagentDispenserDispenseAmount.U1;
                    break;
                case "5":
                    EnergyReagentDispenserDispenseAmount = EnergyReagentDispenserDispenseAmount.U5;
                    break;
                case "10":
                    EnergyReagentDispenserDispenseAmount = EnergyReagentDispenserDispenseAmount.U10;
                    break;
                case "15":
                    EnergyReagentDispenserDispenseAmount = EnergyReagentDispenserDispenseAmount.U15;
                    break;
                case "20":
                    EnergyReagentDispenserDispenseAmount = EnergyReagentDispenserDispenseAmount.U20;
                    break;
                case "25":
                    EnergyReagentDispenserDispenseAmount = EnergyReagentDispenserDispenseAmount.U25;
                    break;
                case "30":
                    EnergyReagentDispenserDispenseAmount = EnergyReagentDispenserDispenseAmount.U30;
                    break;
                case "50":
                    EnergyReagentDispenserDispenseAmount = EnergyReagentDispenserDispenseAmount.U50;
                    break;
                case "100":
                    EnergyReagentDispenserDispenseAmount = EnergyReagentDispenserDispenseAmount.U100;
                    break;
                default:
                    throw new Exception($"Cannot convert the string `{s}` into a valid ReagentDispenser DispenseAmount");
            }
        }
    }

    [Serializable, NetSerializable]
    public sealed class EnergyReagentDispenserDispenseReagentMessage : BoundUserInterfaceMessage
    {
        public readonly string ReagentId;

        public EnergyReagentDispenserDispenseReagentMessage(string reagentId)
        {
            ReagentId = reagentId;
        }
    }

    [Serializable, NetSerializable]
    public sealed class EnergyReagentDispenserClearContainerSolutionMessage : BoundUserInterfaceMessage
    {

    }

    public enum EnergyReagentDispenserDispenseAmount
    {
        U1 = 1,
        U5 = 5,
        U10 = 10,
        U15 = 15,
        U20 = 20,
        U25 = 25,
        U30 = 30,
        U50 = 50,
        U100 = 100,
    }

    [Serializable, NetSerializable]
    public sealed class EnergyReagentInventoryItem(string reagentId, string reagentLabel, float powerCostPerUnit, Color reagentColor)
    {
        public string ReagentId = reagentId;
        public string ReagentLabel = reagentLabel;
        public float PowerCostPerUnit = powerCostPerUnit;
        public Color ReagentColor = reagentColor;
    }

    [Serializable, NetSerializable]
    public sealed class EnergyReagentDispenserBoundUserInterfaceState : BoundUserInterfaceState
    {
        public readonly ContainerInfo? OutputContainer;

        public readonly NetEntity? OutputContainerEntity;

        /// <summary>
        /// A list of the reagents which this dispenser can dispense.
        /// </summary>
        public readonly List<EnergyReagentInventoryItem> Inventory;

        public readonly EnergyReagentDispenserDispenseAmount SelectedDispenseAmount;
        public readonly float BatteryCharge;
        public readonly float BatteryMaxCharge;

        public EnergyReagentDispenserBoundUserInterfaceState(ContainerInfo? outputContainer, NetEntity? outputContainerEntity, List<EnergyReagentInventoryItem> inventory, EnergyReagentDispenserDispenseAmount selectedDispenseAmount, float batteryCharge, float batteryMaxCharge)
        {
            OutputContainer = outputContainer;
            OutputContainerEntity = outputContainerEntity;
            Inventory = inventory;
            SelectedDispenseAmount = selectedDispenseAmount;
            BatteryCharge = batteryCharge;
            BatteryMaxCharge = batteryMaxCharge;
        }
    }

    [Serializable, NetSerializable]
    public enum EnergyReagentDispenserUiKey
    {
        Key
    }
}
