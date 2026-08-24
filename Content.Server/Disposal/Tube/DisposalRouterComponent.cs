// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;

namespace Content.Server.Disposal.Tube
{
    [RegisterComponent]
    [Access(typeof(DisposalTubeSystem))]
    public sealed partial class DisposalRouterComponent : DisposalJunctionComponent
    {
        [DataField("tags")]
        public HashSet<string> Tags = new();

        [DataField("clickSound")]
        public SoundSpecifier ClickSound = new SoundPathSpecifier("/Audio/Machines/machine_switch.ogg");
    }
}