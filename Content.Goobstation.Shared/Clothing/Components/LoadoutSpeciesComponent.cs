// SPDX-FileCopyrightText: 2025 Marty <martynashagriefer@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Roles;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
namespace Content.Goobstation.Shared.Clothing.Components;
/// <summary>
/// Requires LoadoutComponent, replaces starting gear or specific slots with different ones depending on species.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class LoadoutSpeciesComponent : Component
{
    /// <summary>
    /// Key is species prototype ID, value is the StartingGearPrototype to override with.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<string, ProtoId<StartingGearPrototype>> SpeciesStartingGearOverride = new();

    /// <summary>
    /// Slot-specific prototype overrides. Runs after full loadout override.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<string, Dictionary<string, EntProtoId>> SpeciesSlotOverride = new();

    [DataField, AutoNetworkedField]
    public bool Overridden = false;
}

