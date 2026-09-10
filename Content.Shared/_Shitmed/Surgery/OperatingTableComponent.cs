// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Medical.Surgery;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class OperatingTableComponent : Component
{
    [DataField]
    public float SpeedModifier = 1f;

    [DataField, AutoNetworkedField]
    public EntityUid? LinkedScanner;
}