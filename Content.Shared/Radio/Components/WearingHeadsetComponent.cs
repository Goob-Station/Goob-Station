// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.GameStates;

namespace Content.Shared.Radio.Components;

/// <summary>
///     This component is used to tag players that are currently wearing an ACTIVE headset.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class WearingHeadsetComponent : Component
{
    [DataField]
    public EntityUid Headset;
}
