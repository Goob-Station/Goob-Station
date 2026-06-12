// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.MalfAI.Factory;

/// <summary>
/// DoAfter event for the Malf AI building a structure at a location.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class AIBuildDoAfterEvent : DoAfterEvent
{
    public NetCoordinates Location;
    public string Prototype = default!;

    public AIBuildDoAfterEvent() { }

    public AIBuildDoAfterEvent(NetCoordinates location, string prototype)
    {
        Location = location;
        Prototype = prototype;
    }

    public override DoAfterEvent Clone() => new AIBuildDoAfterEvent(Location, Prototype);
}
