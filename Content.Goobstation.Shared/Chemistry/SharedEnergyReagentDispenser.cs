// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Serialization;
using Content.Shared.Chemistry;

namespace Content.Goobstation.Shared.Chemistry
{
    /// <summary>
    /// This class holds constants that are shared between client and server.
    /// </summary>
    public static class SharedEnergyReagentDispenser
    {
        public const string OutputSlotName = "energyBeakerSlot";
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
        public EnergyReagentDispenserSetDispenseAmountMessage(string s)
        {
            EnergyReagentDispenserDispenseAmount = s switch
            {
                "1" => EnergyReagentDispenserDispenseAmount.U1,
                "5" => EnergyReagentDispenserDispenseAmount.U5,
                "10" => EnergyReagentDispenserDispenseAmount.U10,
                "15" => EnergyReagentDispenserDispenseAmount.U15,
                "20" => EnergyReagentDispenserDispenseAmount.U20,
                "25" => EnergyReagentDispenserDispenseAmount.U25,
                "30" => EnergyReagentDispenserDispenseAmount.U30,
                "50" => EnergyReagentDispenserDispenseAmount.U50,
                "100" => EnergyReagentDispenserDispenseAmount.U100,
                _ => throw new Exception(
                    $"Cannot convert the string `{s}` into a valid ReagentDispenser DispenseAmount")
            };
        }
    }

    [Serializable, NetSerializable]
    public sealed class EnergyReagentDispenserDispenseReagentMessage(string reagentId) : BoundUserInterfaceMessage
    {
        public readonly string ReagentId = reagentId;
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
    public sealed class EnergyReagentDispenserBoundUserInterfaceState(
        ContainerInfo? outputContainer,
        NetEntity? outputContainerEntity,
        List<EnergyReagentInventoryItem> inventory,
        EnergyReagentDispenserDispenseAmount selectedDispenseAmount,
        float batteryCharge,
        float batteryMaxCharge,
        float currentReceivingEnergy,
        float idleUse,
        bool usingBattery,
        bool hasPower)
        : BoundUserInterfaceState
    {
        public readonly ContainerInfo? OutputContainer = outputContainer;

        public readonly NetEntity? OutputContainerEntity = outputContainerEntity;

        /// <summary>
        /// A list of the reagents which this dispenser can dispense.
        /// </summary>
        public readonly List<EnergyReagentInventoryItem> Inventory = inventory;

        public readonly EnergyReagentDispenserDispenseAmount SelectedDispenseAmount = selectedDispenseAmount;
        public readonly float BatteryCharge = batteryCharge;
        public readonly float BatteryMaxCharge = batteryMaxCharge;
        public readonly float CurrentReceivingEnergy = currentReceivingEnergy;
        public readonly float IdleUse = idleUse;
        public readonly bool UsingBattery = usingBattery;
        public readonly bool HasPower = hasPower;
    }

    [Serializable, NetSerializable]
    public enum EnergyReagentDispenserUiKey
    {
        Key
    }
}
