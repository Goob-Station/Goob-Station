// SPDX-FileCopyrightText: 2021 CrudeWax <75271456+CrudeWax@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 keronshb <keronshb@live.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Guardian
{
    /// <summary>
    /// Creates a GuardianComponent attached to the user's GuardianHost.
    /// </summary>
    [RegisterComponent]
    public sealed partial class GuardianCreatorComponent : Component
    {
        /// <summary>
        /// Counts as spent upon exhausting the injection
        /// </summary>
        /// <remarks>
        /// We don't mark as deleted as examine depends on this.
        /// </remarks>
        public bool Used = false;

        /// <summary>
        /// The prototype of the guardian entity which will be created
        /// </summary>
        [DataField("guardianProto", customTypeSerializer:typeof(PrototypeIdSerializer<EntityPrototype>), required: true)]
        public string GuardianProto { get; set; } = default!;

        /// <summary>
        /// How long it takes to inject someone.
        /// </summary>
        [DataField("delay")]
        public float InjectionDelay = 5f;
    }
}