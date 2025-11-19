// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Server.Weapons.Ranged.Components
{
    [RegisterComponent]
    public sealed partial class ChemicalAmmoComponent : Component
    {
        public const string DefaultSolutionName = "ammo";

        [DataField("solution")]
        public string SolutionName { get; set; } = DefaultSolutionName;
    }
}
