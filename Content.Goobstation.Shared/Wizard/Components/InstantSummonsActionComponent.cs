// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wizard.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class InstantSummonsActionComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public EntityUid? Entity;
}