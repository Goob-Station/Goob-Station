// SPDX-License-Identifier: MIT

using Content.Shared.Whitelist;
using Content.Shared.Containers.ItemSlots;
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
    public sealed partial class EnergyReagentDispenserComponent : Component
    {
        [DataField]
        public ItemSlot EnergyBeakerSlot = new();

        [DataField]
        public SoundSpecifier ClickSound = new SoundPathSpecifier("/Audio/Machines/machine_switch.ogg");

        [ViewVariables(VVAccess.ReadWrite)]
        public EnergyReagentDispenserDispenseAmount DispenseAmount = EnergyReagentDispenserDispenseAmount.U10;

        [DataField, ViewVariables]
        public SoundSpecifier PowerSound = new SoundPathSpecifier("/Audio/Machines/buzz-sigh.ogg");

        [DataField]
        public Dictionary<string, float> Reagents = [];
    }
}
