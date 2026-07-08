// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Damage.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Medical.Surgery.Conditions;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SurgeryWoundedConditionComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<DamageGroupPrototype> DamageGroup = "Brute";
}
