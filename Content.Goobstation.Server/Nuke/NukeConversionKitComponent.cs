// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;

namespace Content.Goobstation.Server.Nuke;

[RegisterComponent]
public sealed partial class NukeConversionKitComponent : Component
{
    [DataField]
    public SoundSpecifier ConversionSound = new SoundPathSpecifier("/Audio/Machines/terminal_insert_disc.ogg");
}
