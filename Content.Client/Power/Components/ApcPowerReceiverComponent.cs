// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Power.Components;

namespace Content.Client.Power.Components;

[RegisterComponent]
public sealed partial class ApcPowerReceiverComponent : SharedApcPowerReceiverComponent
{
    public override float Load { get; set; }
}
