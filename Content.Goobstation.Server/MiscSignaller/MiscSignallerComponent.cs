// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DeviceLinking;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.MiscSignaller
{
    [RegisterComponent]
    public sealed partial class MiscSignallerComponent : Component
    {
        [DataField]
        public ProtoId<SourcePortPrototype> Port = "Triggered";
       
        [DataField]
        public TimeSpan ActivationInterval = TimeSpan.FromSeconds(3);
       
        public TimeSpan NextActivationWindow;
    }
}
