// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Wizard.Mutate;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BasicHitscanAmmoProviderComponent : AmmoProviderComponent
{
    [DataField(required: true), AutoNetworkedField]
    public EntProtoId Proto;
}
