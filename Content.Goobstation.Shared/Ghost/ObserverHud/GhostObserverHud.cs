// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Ghost.ObserverHud;

/// <summary>
/// Observer HUD options selected in the popup. Multiple options can be combined in one value.
/// </summary>
[Flags]
[Serializable, NetSerializable]
public enum GhostObserverHudVisuals : byte
{
    None = 0,
    JobMindshield = 1 << 0,
    Health = 1 << 1,
    CriminalRecords = 1 << 2,
}

/// <summary>
/// Sent by the client when a checkbox changes so the server can apply the full HUD selection.
/// </summary>
[Serializable, NetSerializable]
public sealed class GhostObserverHudUpdateRequestEvent : EntityEventArgs
{
    public GhostObserverHudVisuals Visuals { get; }

    public GhostObserverHudUpdateRequestEvent(GhostObserverHudVisuals visuals) =>
        Visuals = visuals;
}

public sealed partial class ToggleGhostObserverHudActionEvent : InstantActionEvent { }
