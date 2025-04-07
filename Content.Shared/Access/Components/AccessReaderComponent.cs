// SPDX-FileCopyrightText: 2019 DamianX <DamianX@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2020 FL-OZ <58238103+FL-OZ@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 c4llv07e <38111072+c4llv07e@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 ScarKy0 <106310278+ScarKy0@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.StationRecords;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Access.Components;

/// <summary>
/// Stores access levels necessary to "use" an entity
/// and allows checking if something or somebody is authorized with these access levels.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AccessReaderComponent : Component
{
    /// <summary>
    /// Whether or not the accessreader is enabled.
    /// If not, it will always let people through.
    /// </summary>
    [DataField]
    public bool Enabled = true;

    /// <summary>
    /// The set of tags that will automatically deny an allowed check, if any of them are present.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public HashSet<ProtoId<AccessLevelPrototype>> DenyTags = new();

    /// <summary>
    /// List of access groups that grant access to this reader. Only a single matching group is required to gain access.
    /// A group matches if it is a subset of the set being checked against.
    /// </summary>
    [DataField("access")] [ViewVariables(VVAccess.ReadWrite)]
    public List<HashSet<ProtoId<AccessLevelPrototype>>> AccessLists = new();

    /// <summary>
    /// A list of <see cref="StationRecordKey"/>s that grant access. Only a single matching key is required to gain
    /// access.
    /// </summary>
    [DataField]
    public HashSet<StationRecordKey> AccessKeys = new();

    /// <summary>
    /// If specified, then this access reader will instead pull access requirements from entities contained in the
    /// given container.
    /// </summary>
    /// <remarks>
    /// This effectively causes <see cref="DenyTags"/>, <see cref="AccessLists"/>, and <see cref="AccessKeys"/> to be
    /// ignored, though <see cref="Enabled"/> is still respected. Access is denied if there are no valid entities or
    /// they all deny access.
    /// </remarks>
    [DataField]
    public string? ContainerAccessProvider;

    /// <summary>
    /// A list of past authentications
    /// </summary>
    [DataField]
    public Queue<AccessRecord> AccessLog = new();

    /// <summary>
    /// A limit on the max size of <see cref="AccessLog"/>
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int AccessLogLimit = 20;

    /// <summary>
    /// If true logging on successful access uses will be disabled.
    /// Can be set by LOG wire.
    /// </summary>
    [DataField]
    public bool LoggingDisabled;

    /// <summary>
    /// Whether or not emag interactions have an effect on this.
    /// </summary>
    [DataField]
    public bool BreakOnAccessBreaker = true;
}

[DataDefinition, Serializable, NetSerializable]
public readonly partial record struct AccessRecord(
    [property: DataField, ViewVariables(VVAccess.ReadWrite)]
    TimeSpan AccessTime,
    [property: DataField, ViewVariables(VVAccess.ReadWrite)]
    string Accessor)
{
    public AccessRecord() : this(TimeSpan.Zero, string.Empty)
    {
    }
}

[Serializable, NetSerializable]
public sealed class AccessReaderComponentState : ComponentState
{
    public bool Enabled;

    public HashSet<ProtoId<AccessLevelPrototype>> DenyTags;

    public List<HashSet<ProtoId<AccessLevelPrototype>>> AccessLists;

    public List<(NetEntity, uint)> AccessKeys;

    public Queue<AccessRecord> AccessLog;

    public int AccessLogLimit;

    public AccessReaderComponentState(bool enabled, HashSet<ProtoId<AccessLevelPrototype>> denyTags, List<HashSet<ProtoId<AccessLevelPrototype>>> accessLists, List<(NetEntity, uint)> accessKeys, Queue<AccessRecord> accessLog, int accessLogLimit)
    {
        Enabled = enabled;
        DenyTags = denyTags;
        AccessLists = accessLists;
        AccessKeys = accessKeys;
        AccessLog = accessLog;
        AccessLogLimit = accessLogLimit;
    }
}

public sealed class AccessReaderConfigurationChangedEvent : EntityEventArgs
{
    public AccessReaderConfigurationChangedEvent()
    {
    }
}