// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Server.Disposal.Tube
{
    // TODO: Different types of tubes eject in random direction with no exit point
    [RegisterComponent]
    [Access(typeof(DisposalTubeSystem))]
    [Virtual]
    public partial class DisposalTransitComponent : Component
    {
    }
}
