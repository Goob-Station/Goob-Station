// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class HereticBladeComponent : Component
{
    [DataField]
    public string? Path;

    [DataField]
    public SoundSpecifier? ShatterSound = new SoundCollectionSpecifier("GlassBreak");

    [DataField]
    public SoundSpecifier ArrivalSound = new SoundPathSpecifier("/Audio/Effects/teleport_arrival.ogg");

    [DataField]
    public SoundSpecifier DepartureSound = new SoundPathSpecifier("/Audio/Effects/teleport_departure.ogg");
}
