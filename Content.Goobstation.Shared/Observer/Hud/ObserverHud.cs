// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Observer.Hud;

/// <summary>
/// Sent by the client when a checkbox changes so the server can apply the full HUD selection.
/// </summary>
[Serializable, NetSerializable]
public sealed class ObserverHudUpdateRequestEvent : EntityEventArgs
{
    public bool ShowJobMindshield { get; }
    public bool ShowHealth { get; }
    public bool ShowCriminalRecords { get; }

    public ObserverHudUpdateRequestEvent(
        bool showJobMindshield,
        bool showHealth,
        bool showCriminalRecords)
    {
        ShowJobMindshield = showJobMindshield;
        ShowHealth = showHealth;
        ShowCriminalRecords = showCriminalRecords;
    }
}

public sealed partial class ToggleObserverHudActionEvent : InstantActionEvent { }
