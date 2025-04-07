// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Storage;

namespace Content.Server.GameTicking.Rules.VariationPass.Components;

/// <summary>
/// This is used for replacing a certain amount of entities with other entities in a variation pass.
///
/// </summary>
/// <remarks>
/// POTENTIALLY REPLACEABLE ENTITIES MUST BE MARKED WITH A REPLACEMENT MARKER
/// AND HAVE A SYSTEM INHERITING FROM <see cref="BaseEntityReplaceVariationPassSystem{TEntComp,TGameRuleComp}"/>
/// SEE <see cref="WallReplaceVariationPassSystem"/>
/// </remarks>
[RegisterComponent]
public sealed partial class EntityReplaceVariationPassComponent : Component
{
    /// <summary>
    ///     Number of matching entities before one will be replaced on average.
    /// </summary>
    [DataField(required: true)]
    public float EntitiesPerReplacementAverage;

    [DataField(required: true)]
    public float EntitiesPerReplacementStdDev;

    /// <summary>
    ///     Prototype(s) to replace matched entities with.
    /// </summary>
    [DataField(required: true)]
    public List<EntitySpawnEntry> Replacements = default!;
}