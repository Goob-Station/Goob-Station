// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Server.Electrocution
{
    [RegisterComponent]
    public sealed partial class RandomInsulationComponent : Component
    {
        [DataField("list")]
        public float[] List = { 0f };
    }
}
