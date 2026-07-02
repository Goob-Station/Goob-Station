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
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Storage;
using Robust.Shared.Serialization;

namespace Content.Shared.Chemistry
{
    /// <summary>
    /// This class holds constants that are shared between client and server.
    /// </summary>
    public sealed class SharedReagentDispenser
    {
        public const string OutputSlotName = "beakerSlot";
        #region Pirate: chem recipes
        public const string RecipeDiskSlotName = "recipeDiskSlot";
        public const int RecipeNameMaxLength = 16;
        #endregion
    }

    [Serializable, NetSerializable]
    public sealed class ReagentDispenserSetDispenseAmountMessage : BoundUserInterfaceMessage
    {
        public readonly ReagentDispenserDispenseAmount ReagentDispenserDispenseAmount;

        public ReagentDispenserSetDispenseAmountMessage(ReagentDispenserDispenseAmount amount)
        {
            ReagentDispenserDispenseAmount = amount;
        }

        /// <summary>
        ///     Create a new instance from interpreting a String as an integer,
        ///     throwing an exception if it is unable to parse.
        /// </summary>
        public ReagentDispenserSetDispenseAmountMessage(String s)
        {
            switch (s)
            {
                case "1":
                    ReagentDispenserDispenseAmount = ReagentDispenserDispenseAmount.U1;
                    break;
                case "5":
                    ReagentDispenserDispenseAmount = ReagentDispenserDispenseAmount.U5;
                    break;
                case "10":
                    ReagentDispenserDispenseAmount = ReagentDispenserDispenseAmount.U10;
                    break;
                case "15":
                    ReagentDispenserDispenseAmount = ReagentDispenserDispenseAmount.U15;
                    break;
                case "20":
                    ReagentDispenserDispenseAmount = ReagentDispenserDispenseAmount.U20;
                    break;
                case "25":
                    ReagentDispenserDispenseAmount = ReagentDispenserDispenseAmount.U25;
                    break;
                case "30":
                    ReagentDispenserDispenseAmount = ReagentDispenserDispenseAmount.U30;
                    break;
                case "50":
                    ReagentDispenserDispenseAmount = ReagentDispenserDispenseAmount.U50;
                    break;
                case "100":
                    ReagentDispenserDispenseAmount = ReagentDispenserDispenseAmount.U100;
                    break;
                default:
                    throw new Exception($"Cannot convert the string `{s}` into a valid ReagentDispenser DispenseAmount");
            }
        }
    }

    [Serializable, NetSerializable]
    public sealed class ReagentDispenserDispenseReagentMessage : BoundUserInterfaceMessage
    {
        public readonly ItemStorageLocation StorageLocation;

        public ReagentDispenserDispenseReagentMessage(ItemStorageLocation storageLocation)
        {
            StorageLocation = storageLocation;
        }
    }

    /// <summary>
    ///     Message sent by the user interface to ask the reagent dispenser to eject a container
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class ReagentDispenserEjectContainerMessage : BoundUserInterfaceMessage
    {
        public readonly ItemStorageLocation StorageLocation;

        public ReagentDispenserEjectContainerMessage(ItemStorageLocation storageLocation)
        {
            StorageLocation = storageLocation;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ReagentDispenserClearContainerSolutionMessage : BoundUserInterfaceMessage
    {

    }

    #region Pirate: chem plumbing
    [Serializable, NetSerializable]
    public sealed class ReagentDispenserToggleValveMessage : BoundUserInterfaceMessage
    {
    }
    #endregion

    public enum ReagentDispenserDispenseAmount
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
    public sealed class ReagentInventoryItem(ItemStorageLocation storageLocation, string reagentLabel, FixedPoint2 quantity, Color reagentColor)
    {
        public ItemStorageLocation StorageLocation = storageLocation;
        public string ReagentLabel = reagentLabel;
        public FixedPoint2 Quantity = quantity;
        public Color ReagentColor = reagentColor;
    }

    [Serializable, NetSerializable]
    public sealed class ReagentDispenserBoundUserInterfaceState : BoundUserInterfaceState
    {
        public readonly ContainerInfo? OutputContainer;

        public readonly NetEntity? OutputContainerEntity;

        /// <summary>
        /// A list of the reagents which this dispenser can dispense.
        /// </summary>
        public readonly List<ReagentInventoryItem> Inventory;

        public readonly ReagentDispenserDispenseAmount SelectedDispenseAmount;

        public readonly bool ValveOpen; // Pirate: chem plumbing

        public readonly List<ReagentDispenserRecipeItem> SavedRecipes;// Pirate: chem recipes
        public readonly bool HasRecipeDisk;// Pirate: chem recipes
        public readonly List<ReagentDispenserRecipeItem> DiskRecipes;// Pirate: chem recipes
        public readonly bool IsRecordingRecipe;// Pirate: chem recipes
        public readonly List<ReagentQuantity> RecordingRecipeReagents;// Pirate: chem recipes

        public ReagentDispenserBoundUserInterfaceState( // Pirate: chem recipes
            ContainerInfo? outputContainer,
            NetEntity? outputContainerEntity,
            List<ReagentInventoryItem> inventory,
            ReagentDispenserDispenseAmount selectedDispenseAmount,
            bool valveOpen, // Pirate: chem plumbing
            List<ReagentDispenserRecipeItem> savedRecipes,
            bool hasRecipeDisk,
            List<ReagentDispenserRecipeItem> diskRecipes,
            bool isRecordingRecipe,
            List<ReagentQuantity> recordingRecipeReagents)
        {
            OutputContainer = outputContainer;
            OutputContainerEntity = outputContainerEntity;
            Inventory = inventory;
            SelectedDispenseAmount = selectedDispenseAmount;
            ValveOpen = valveOpen; // Pirate: chem plumbing
            #region Pirate: chem recipes
            SavedRecipes = savedRecipes;
            HasRecipeDisk = hasRecipeDisk;
            DiskRecipes = diskRecipes;
            IsRecordingRecipe = isRecordingRecipe;
            RecordingRecipeReagents = recordingRecipeReagents;
            #endregion
        }
    }

    [Serializable, NetSerializable]
    public enum ReagentDispenserUiKey
    {
        Key
    }
    #region Pirate: chem recipes
    [Serializable, NetSerializable]
    public sealed class ReagentDispenserRecipeItem(string name, Color color)
    {
        public string Name = name;
        public Color Color = color;
    }
    [Serializable, NetSerializable]
    public sealed class ReagentDispenserStartRecipeRecordingMessage : BoundUserInterfaceMessage
    {
    }

    [Serializable, NetSerializable]
    public sealed class ReagentDispenserCancelRecipeRecordingMessage : BoundUserInterfaceMessage
    {
    }

    [Serializable, NetSerializable]
    public sealed class ReagentDispenserSaveRecipeMessage : BoundUserInterfaceMessage
    {
        public readonly string Name;

        public ReagentDispenserSaveRecipeMessage(string name)
        {
            Name = name;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ReagentDispenserDispenseRecipeMessage : BoundUserInterfaceMessage
    {
        public readonly string Name;

        public ReagentDispenserDispenseRecipeMessage(string name)
        {
            Name = name;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ReagentDispenserDeleteRecipeMessage : BoundUserInterfaceMessage
    {
        public readonly string Name;

        public ReagentDispenserDeleteRecipeMessage(string name)
        {
            Name = name;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ReagentDispenserClearRecipesMessage : BoundUserInterfaceMessage
    {
    }

    [Serializable, NetSerializable]
    public sealed class ReagentDispenserSaveRecipeToDiskMessage : BoundUserInterfaceMessage
    {
        public readonly string Name;

        public ReagentDispenserSaveRecipeToDiskMessage(string name)
        {
            Name = name;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ReagentDispenserCopyDiskRecipeMessage : BoundUserInterfaceMessage
    {
        public readonly string Name;

        public ReagentDispenserCopyDiskRecipeMessage(string name)
        {
            Name = name;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ReagentDispenserDispenseDiskRecipeMessage : BoundUserInterfaceMessage
    {
        public readonly string Name;

        public ReagentDispenserDispenseDiskRecipeMessage(string name)
        {
            Name = name;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ReagentDispenserDeleteDiskRecipeMessage : BoundUserInterfaceMessage
    {
        public readonly string Name;

        public ReagentDispenserDeleteDiskRecipeMessage(string name)
        {
            Name = name;
        }
    }
    #endregion
}
