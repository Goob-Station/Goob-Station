// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aineias1 <dmitri.s.kiselev@gmail.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 McBosserson <148172569+McBosserson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <plmilonpl@gmail.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Unlumination <144041835+Unlumy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Lavaland.Shuttles.Systems;
using Content.Shared.Tag;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Shuttles.Components;

/// <summary>
/// A shuttle console that can only ftl-dock between 2 grids.
/// The shuttle used must have <see cref="DockingShuttleComponent"/>.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedDockingConsoleSystem))]
[AutoGenerateComponentState]
public sealed partial class DockingConsoleComponent : Component
{
    /// <summary>
    /// Title of the window to use
    /// </summary>
    [DataField(required: true)]
    public LocId WindowTitle;

    /// <summary>
    /// A whitelist the shuttle has to match to be piloted.
    /// </summary>
    [DataField(required: true)]
    public EntityWhitelist ShuttleWhitelist = new();

    /// <summary>
    /// The shuttle that matches <see cref="ShuttleWhitelist"/>.
    /// If this is null a shuttle was not found and this console does nothing.
    /// </summary>
    [DataField]
    public EntityUid? Shuttle;

    /// <summary>
    /// Whether <see cref="Shuttle"/> is set on the server or not.
    /// Client can't use Shuttle outside of PVS range so that isn't networked.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool HasShuttle;
}