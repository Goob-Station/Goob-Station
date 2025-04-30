// SPDX-FileCopyrightText: 2021 ike709 <ike709@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 ike709 <sparebytes@protonmail.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Prototypes;

namespace Content.Shared.HUD
{
    [Prototype("hudTheme")]
    public sealed partial class HudThemePrototype : IPrototype, IComparable<HudThemePrototype>
    {
        [DataField("name", required: true)]
        public string Name { get; private set; } = string.Empty;

        [IdDataField]
        public string ID { get; private set; } = string.Empty;

        [DataField("path", required: true)]
        public string Path { get; private set; } = string.Empty;

        /// <summary>
        /// An order for the themes to be displayed in the UI
        /// </summary>
        [DataField]
        public int Order = 0;

        public int CompareTo(HudThemePrototype? other)
        {
            return Order.CompareTo(other?.Order);
        }
    }
}