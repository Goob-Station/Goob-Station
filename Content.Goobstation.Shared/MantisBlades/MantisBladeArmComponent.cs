// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.MantisBlades;

/// <summary>
/// An arm with a mantis blade built in. This will also be required later for the cyberware UI.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MantisBladeArmComponent : Component
{
    public const string BladeContainer = "mantis-blade";

    /// <summary>
    /// The blade weapon spawned inside this arm.
    /// </summary>
    [DataField]
    public EntProtoId BladeProto = "MantisBlade";

    [DataField, AutoNetworkedField]
    public EntityUid? Blade;
}
