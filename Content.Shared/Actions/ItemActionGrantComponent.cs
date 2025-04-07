// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Inventory;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Actions;

/// <summary>
/// Works in tandem with <see cref="ActionGrantComponent"/> by granting those actions to the equipper entity.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(ActionGrantSystem))]
public sealed partial class ItemActionGrantComponent : Component
{
    [DataField(required: true), AutoNetworkedField, AlwaysPushInheritance]
    public List<EntProtoId> Actions = new();

    // Goobstation start
    [DataField]
    public bool RestrictSlots;

    [DataField]
    public SlotFlags? RestrictedSlots;
    // Goobstation end
}