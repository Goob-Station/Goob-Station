// SPDX-License-Identifier: MIT

using Content.Shared.Whitelist;
using Content.Shared.Containers.ItemSlots;
using Content.Server._Pirate.Chemistry; // Pirate: chem recipes
using Content.Goobstation.Maths.FixedPoint; // Pirate: chem recipes
using Content.Goobstation.Server.Chemistry.EntitySystems;
using Content.Goobstation.Shared.Chemistry;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Goobstation.Server.Chemistry.Components
{
    /// <summary>
    /// A machine that dispenses reagents into a solution container from containers in its storage slots.
    /// </summary>
    [RegisterComponent]
    [Access(typeof(EnergyReagentDispenserSystem))]
    public sealed partial class EnergyReagentDispenserComponent : Component, IPirateRecipeDispenserComponent // Pirate: chem recipes
    {
        [DataField]
        public ItemSlot EnergyBeakerSlot = new();
        [DataField]
        public ItemSlot RecipeDiskSlot = new(); // Pirate: chem recipes

        [DataField]
        public SoundSpecifier ClickSound = new SoundPathSpecifier("/Audio/Machines/machine_switch.ogg");

        [ViewVariables(VVAccess.ReadWrite)]
        public EnergyReagentDispenserDispenseAmount DispenseAmount = EnergyReagentDispenserDispenseAmount.U10;

        [DataField, ViewVariables]
        public SoundSpecifier PowerSound = new SoundPathSpecifier("/Audio/Machines/buzz-sigh.ogg");

        [DataField]
        public Dictionary<string, float> Reagents = [];
        #region Pirate: chem recipes
        [DataField]
        public SoundSpecifier ErrorSound = new SoundPathSpecifier("/Audio/_Pirate/Machines/terminal_error.ogg");
        [ViewVariables]
        public Dictionary<string, Dictionary<string, FixedPoint2>> SavedRecipes = new();

        [ViewVariables]
        public Dictionary<string, FixedPoint2>? RecordingRecipe;

        Dictionary<string, Dictionary<string, FixedPoint2>> IPirateRecipeDispenserComponent.SavedRecipes => SavedRecipes;
        Dictionary<string, FixedPoint2>? IPirateRecipeDispenserComponent.RecordingRecipe
        {
            get => RecordingRecipe;
            set => RecordingRecipe = value;
        }
        SoundSpecifier IPirateRecipeDispenserComponent.ClickSound => ClickSound;
        SoundSpecifier IPirateRecipeDispenserComponent.ErrorSound => ErrorSound;
        #endregion
    }
}
