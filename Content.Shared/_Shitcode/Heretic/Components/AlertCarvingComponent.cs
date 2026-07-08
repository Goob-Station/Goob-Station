// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class AlertCarvingComponent : Component
{
    [DataField]
    public EntityUid? User;

    [DataField]
    public SoundSpecifier? AlertSound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/curse.ogg");

    [DataField]
    public int TeleportDelay = 5000;
}
