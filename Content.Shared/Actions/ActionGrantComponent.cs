// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Actions;

/// <summary>
/// Grants actions on MapInit and removes them on shutdown
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(ActionGrantSystem))]
public sealed partial class ActionGrantComponent : Component
{
    [DataField(required: true), AutoNetworkedField, AlwaysPushInheritance]
    public List<EntProtoId> Actions = new();

    [DataField, AutoNetworkedField]
    public List<EntityUid> ActionEntities = new();
}