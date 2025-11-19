// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.Audio;

namespace Content.Server.Disposal.Tube
{
    [RegisterComponent]
    public sealed partial class DisposalTaggerComponent : DisposalTransitComponent
    {
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("tag")]
        public string Tag = "";

        [DataField("clickSound")]
        public SoundSpecifier ClickSound = new SoundPathSpecifier("/Audio/Machines/machine_switch.ogg");
    }
}
