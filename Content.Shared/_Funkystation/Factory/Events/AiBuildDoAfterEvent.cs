// SPDX-License-Identifier: MIT

using System;
using Content.Shared.DoAfter;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;
using Robust.Shared.Map;

namespace Content.Shared._Funkystation.Factory.Events;

/// <summary>
/// Networked DoAfter payload for AI build action; must be in Shared due to NetSerializable.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class AiBuildDoAfterEvent : DoAfterEvent
{
    public NetCoordinates Location;
    public string Prototype = string.Empty;
    public NetEntity? Effect;

    public AiBuildDoAfterEvent() { }

    public AiBuildDoAfterEvent(NetCoordinates location, string prototype, NetEntity? effect)
    {
        Location = location;
        Prototype = prototype;
        Effect = effect;
    }

    public override DoAfterEvent Clone() => new AiBuildDoAfterEvent(Location, Prototype, Effect);
}
