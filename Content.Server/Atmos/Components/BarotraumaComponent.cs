// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 chairbender <kwhipke1@gmail.com>
// SPDX-FileCopyrightText: 2020 creadth <creadth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2021 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Silver <silvertorch5@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 LordEclipse <106132477+LordEclipse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Menshin <Menshin@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Server.Atmos.Components
{
    /// <summary>
    ///     Barotrauma: injury because of changes in air pressure.
    /// </summary>
    [RegisterComponent]
    public sealed partial class BarotraumaComponent : Component
    {
        [DataField("damage", required: true)]
        [ViewVariables(VVAccess.ReadWrite)]
        public DamageSpecifier Damage = default!;

        [DataField("maxDamage")]
        [ViewVariables(VVAccess.ReadWrite)]
        public FixedPoint2 MaxDamage = 200;

        /// <summary>
        ///     Used to keep track of when damage starts/stops. Useful for logs.
        /// </summary>
        public bool TakingDamage = false;

        /// <summary>
        ///     These are the inventory slots that are checked for pressure protection. If a slot is missing protection, no protection is applied.
        /// </summary>
        [DataField("protectionSlots")]
        public List<string> ProtectionSlots = new() { "head", "outerClothing" };

        /// <summary>
        /// Cached pressure protection values
        /// </summary>
        [ViewVariables]
        public float HighPressureMultiplier = 1f;
        [ViewVariables]
        public float HighPressureModifier = 0f;
        [ViewVariables]
        public float LowPressureMultiplier = 1f;
        [ViewVariables]
        public float LowPressureModifier = 0f;

        /// <summary>
        /// Whether the entity is immuned to pressure (i.e possess the PressureImmunity component)
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public bool HasImmunity = false;

        [DataField]
        public ProtoId<AlertPrototype> HighPressureAlert = "HighPressure";

        [DataField]
        public ProtoId<AlertPrototype> LowPressureAlert = "LowPressure";

        [DataField]
        public ProtoId<AlertCategoryPrototype> PressureAlertCategory = "Pressure";
    }
}