// SPDX-FileCopyrightText: 2025 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tayrtahn <tayrtahn@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Dataset;
using Robust.Shared.Prototypes;

namespace Content.Shared.Preferences.Loadouts;

/// <summary>
/// Corresponds to a Job / Antag prototype and specifies loadouts
/// </summary>
[Prototype]
public sealed partial class RoleLoadoutPrototype : IPrototype
{
    /*
     * Separate to JobPrototype / AntagPrototype as they are turning into messy god classes.
     */

    [IdDataField]
    public string ID { get; private set; } = string.Empty;

    /// <summary>
    /// Can the user edit their entity name for this role loadout?
    /// </summary>
    [DataField]
    public bool CanCustomizeName;

    /// <summary>
    /// Should we use a random name for this loadout?
    /// </summary>
    [DataField]
    public ProtoId<LocalizedDatasetPrototype>? NameDataset;

    // Not required so people can set their names.
    /// <summary>
    /// Groups that comprise this role loadout.
    /// </summary>
    [DataField]
    public List<ProtoId<LoadoutGroupPrototype>> Groups = new();

    /// <summary>
    /// How many points are allotted for this role loadout prototype.
    /// </summary>
    [DataField]
    public int? Points;
}