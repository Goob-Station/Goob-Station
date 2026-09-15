// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Oskarrr.WorkingJoe;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class WorkingJoeVoiceComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId Action = "ActionWorkingJoeVoice";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;
}
