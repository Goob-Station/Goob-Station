// SPDX-FileCopyrightText: 2025 Moony33 <ultimoprmo@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.NewCooking.NewCookingComponent
{
    [RegisterComponent]
    public sealed partial class CookingMachinesComponent : Component
    {
        /// <summary>
        /// is the machine turned on?
        /// </summary>
        [DataField]
        public bool IsOn = false;
        /// <summary>
        /// does the machine have food inside?
        /// </summary>
        [DataField]
        public bool ContainsFood = false;
        /// <summary>
        /// if the machine can be used as an oven
        /// </summary>
        [DataField]
        public bool IsOven = false;
    }
}