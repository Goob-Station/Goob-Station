// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Solar
{
    [Serializable, NetSerializable]
    public sealed class SolarControlConsoleBoundInterfaceState : BoundUserInterfaceState
    {
        /// <summary>
        /// The target rotation of the panels in radians.
        /// </summary>
        public Angle Rotation;

        /// <summary>
        /// The target velocity of the panels in radians/minute.
        /// </summary>
        public Angle AngularVelocity;

        /// <summary>
        /// The total amount of power the panels are supplying.
        /// </summary>
        public float OutputPower;

        /// <summary>
        /// The current sun angle.
        /// </summary>
        public Angle TowardsSun;

        public SolarControlConsoleBoundInterfaceState(Angle r, Angle vm, float p, Angle tw)
        {
            Rotation = r;
            AngularVelocity = vm;
            OutputPower = p;
            TowardsSun = tw;
        }
    }

    [Serializable, NetSerializable]
    public sealed class SolarControlConsoleAdjustMessage : BoundUserInterfaceMessage
    {
        /// <summary>
        /// New target rotation of the panels in radians.
        /// </summary>
        public Angle Rotation;

        /// <summary>
        /// New target velocity of the panels in radians/second.
        /// </summary>
        public Angle AngularVelocity;
    }

    [Serializable, NetSerializable]
    public enum SolarControlConsoleUiKey
    {
        Key
    }
}