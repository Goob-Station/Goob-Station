// SPDX-FileCopyrightText: 2022 EmoGarbage404 <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Bakke <luringens@protonmail.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Polymorph.Systems;
using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;

namespace Content.Server.Polymorph.Components;

[RegisterComponent]
[Access(typeof(PolymorphSystem))]
public sealed partial class PolymorphableComponent : Component
{
    /// <summary>
    /// A list of all the polymorphs that the entity has.
    /// Used to manage them and remove them if needed.
    /// </summary>
    public Dictionary<ProtoId<PolymorphPrototype>, EntityUid>? PolymorphActions = null;

    /// <summary>
    /// Timestamp for when the most recent polymorph ended.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan? LastPolymorphEnd = null;

        /// <summary>
    /// The polymorphs that the entity starts out being able to do.
    /// </summary>
    [DataField]
    public List<ProtoId<PolymorphPrototype>>? InnatePolymorphs;
}