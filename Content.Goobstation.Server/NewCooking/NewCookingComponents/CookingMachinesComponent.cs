using System;
using Content.Server.Construction.Components;
using Robust.Shared.Player;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;


namespace Content.Goobstation.Server.NewCooking.NewCookingComponent
{
    [RegisterComponent]
    [NetworkedComponent]
    public sealed partial class CookingMachinesComponent : Component
    {
        /// <summary>
        /// is the machine turned on?
        /// </summary>
        [DataField("isOn")]
        public bool IsOn = false;
        /// <summary>
        /// does the machine contain food?
        /// </summary>
        [DataField("containsFood")]
        public EntityUid? ContainsFood;
        /// <summary>
        /// if the step needs an oven
        /// </summary>
        [DataField("requiresOven")]
        public bool CookingRequireOven = false;

        /// <summary>
        /// if the machine can be used as an oven
        /// </summary>
        [DataField("isOven")]
        public bool IsOven = false;

        [ViewVariables]
        public List<EntityUid> Ingredients = new();
    }
}