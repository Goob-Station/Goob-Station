// SPDX-License-Identifier: MIT

using Content.Shared.Whitelist;
using Content.Shared.Containers.ItemSlots;
using Content.Server._Pirate.Chemistry; // Pirate: chem recipes
using Content.Goobstation.Maths.FixedPoint; // Pirate: chem recipes
using Content.Server.Chemistry.EntitySystems;
using Content.Shared.Chemistry;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Chemistry.Components
{
    /// <summary>
    /// A machine that dispenses reagents into a solution container from containers in its storage slots.
    /// </summary>
    [RegisterComponent]
    [Access(typeof(ReagentDispenserSystem))]
    public sealed partial class ReagentDispenserComponent : Component, IPirateRecipeDispenserComponent // Pirate: chem recipes
    {
        [DataField]
        public ItemSlot BeakerSlot = new();

        [DataField("clickSound"), ViewVariables(VVAccess.ReadWrite)]
        public SoundSpecifier ClickSound = new SoundPathSpecifier("/Audio/Machines/machine_switch.ogg");

        [ViewVariables(VVAccess.ReadWrite)]
        public ReagentDispenserDispenseAmount DispenseAmount = ReagentDispenserDispenseAmount.U10;

        #region Pirate: chem recipes
        [DataField]
        public ItemSlot RecipeDiskSlot = new();
        [ViewVariables]
        public Dictionary<string, Dictionary<string, FixedPoint2>> SavedRecipes = new();
        [ViewVariables]
        public Dictionary<string, FixedPoint2>? RecordingRecipe;

        [DataField("errorSound"), ViewVariables(VVAccess.ReadWrite)]
        public SoundSpecifier ErrorSound = new SoundPathSpecifier("/Audio/_Pirate/Machines/terminal_error.ogg");

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
