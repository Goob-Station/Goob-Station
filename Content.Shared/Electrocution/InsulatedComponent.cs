// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Electrocution
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    [Access(typeof(SharedElectrocutionSystem))]
    public sealed partial class InsulatedComponent : Component
    {
        // Technically, people could cheat and figure out which budget insulated gloves are gud and which ones are bad.
        // We might want to rethink this a little bit.
        /// <summary>
        ///     Siemens coefficient. Zero means completely insulated.
        /// </summary>
        [DataField, AutoNetworkedField]
        public float Coefficient { get; set; } = 0f;
    }
}