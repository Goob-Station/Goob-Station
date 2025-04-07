// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Abilities;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CrawlUnderObjectsComponent : Component
{
    [DataField]
    public EntityUid? ToggleHideAction;

    [DataField]
    public EntProtoId? ActionProto;

    [DataField]
    public bool Enabled = false;

    /// <summary>
    ///     List of fixtures that had their collision mask changed.
    ///     Required for re-adding the collision mask.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<(string key, int originalMask)> ChangedFixtures = new();

    [DataField]
    public int? OriginalDrawDepth;

    [DataField]
    public float SneakSpeedModifier = 0.7f;
}

[Serializable, NetSerializable]
public enum SneakMode : byte
{
    Enabled
}

public sealed partial class ToggleCrawlingStateEvent : InstantActionEvent { }

[Serializable, NetSerializable]
public sealed partial class CrawlingUpdatedEvent(bool enabled = false) : EventArgs
{
    public readonly bool Enabled = enabled;
}