// SPDX-FileCopyrightText: 2022 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Nutrition.EntitySystems;
using Content.Shared.Containers.ItemSlots;

namespace Content.Server.Nutrition.Components
{
    /// <summary>
    ///     A reusable vessel for smoking
    /// </summary>
    [RegisterComponent, Access(typeof(SmokingSystem))]
    public sealed partial class SmokingPipeComponent : Component
    {
        public const string BowlSlotId = "bowl_slot";

        [DataField("bowl_slot")]
        public ItemSlot BowlSlot = new();
    }
}