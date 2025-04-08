// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <154002422+LuciferEOS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations;

namespace Content.Goobstation.Shared.Clothing.Components
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class DamageOverTimeComponent : Component
    {
        [DataField("damage", required: true)]
        public DamageSpecifier Damage { get; set; } = new();

        [DataField("interval", customTypeSerializer: typeof(TimespanSerializer))]
        public TimeSpan Interval = TimeSpan.FromSeconds(1);

        [DataField("ignoreResistances")]
        public bool IgnoreResistances { get; set; } = false;

        [DataField]
        public TimeSpan NextTickTime = TimeSpan.Zero;
    }
}