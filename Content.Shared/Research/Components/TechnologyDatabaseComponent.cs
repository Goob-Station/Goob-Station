// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Lathe;
using Content.Shared.Research.Prototypes;
using Content.Shared.Research.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared.Research.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedResearchSystem), typeof(SharedLatheSystem)), AutoGenerateComponentState]
public sealed partial class TechnologyDatabaseComponent : Component
{
    /// <summary>
    /// A main discipline that locks out other discipline technology past a certain tier.
    /// </summary>
    [AutoNetworkedField]
    [DataField("mainDiscipline", customTypeSerializer: typeof(PrototypeIdSerializer<TechDisciplinePrototype>))]
    public string? MainDiscipline;

    [AutoNetworkedField]
    [DataField("currentTechnologyCards")]
    public List<string> CurrentTechnologyCards = new();

    /// <summary>
    /// Which research disciplines are able to be unlocked
    /// </summary>
    [AutoNetworkedField]
    [DataField("supportedDisciplines", customTypeSerializer: typeof(PrototypeIdListSerializer<TechDisciplinePrototype>))]
    public List<string> SupportedDisciplines = new();

    /// <summary>
    /// The ids of all the technologies which have been unlocked.
    /// </summary>
    [AutoNetworkedField]
    [DataField("unlockedTechnologies", customTypeSerializer: typeof(PrototypeIdListSerializer<TechnologyPrototype>))]
    public List<string> UnlockedTechnologies = new();

    /// <summary>
    /// The ids of all the lathe recipes which have been unlocked.
    /// This is maintained alongside the TechnologyIds
    /// </summary>
    /// todo: if you unlock all the recipes in a tech, it doesn't count as unlocking the tech. sadge
    [AutoNetworkedField]
    [DataField("unlockedRecipes", customTypeSerializer: typeof(PrototypeIdListSerializer<LatheRecipePrototype>))]
    public List<string> UnlockedRecipes = new();
}

/// <summary>
/// Event raised on the database whenever its
/// technologies or recipes are modified.
/// </summary>
/// <remarks>
/// This event is forwarded from the
/// server to all of it's clients.
/// </remarks>
[ByRefEvent]
public readonly record struct TechnologyDatabaseModifiedEvent // Goobstation - Lathe message on recipes update
{
    public readonly List<ProtoId<LatheRecipePrototype>> UnlockedRecipes;

    public TechnologyDatabaseModifiedEvent(List<ProtoId<LatheRecipePrototype>>? unlockedRecipes = null)
    {
        UnlockedRecipes = unlockedRecipes ?? new();
    }
};

/// <summary>
/// Event raised on a database after being synchronized
/// with the values from another database.
/// </summary>
[ByRefEvent]
public readonly record struct TechnologyDatabaseSynchronizedEvent;