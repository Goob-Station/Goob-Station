// SPDX-FileCopyrightText: 2021 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2021 E F R <602406+Efruit@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Rotatable
{
    [RegisterComponent]
    public sealed partial class RotatableComponent : Component
    {
        /// <summary>
        ///     If true, this entity can be rotated even while anchored.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("rotateWhileAnchored")]
        public bool RotateWhileAnchored { get; private set; }

        /// <summary>
        ///     If true, will rotate entity in players direction when pulled
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("rotateWhilePulling")]
        public bool RotateWhilePulling { get; private set; } = true;

        /// <summary>
        ///     The angular value to change when using the rotate verbs.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("increment")]
        public Angle Increment { get; private set; } = Angle.FromDegrees(90);
    }
}