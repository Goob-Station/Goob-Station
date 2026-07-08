// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared.Medical.Cryogenics;

[RegisterComponent]
[NetworkedComponent]
public sealed partial class InsideCryoPodComponent: Component
{
    [ViewVariables]
    [DataField("previousOffset")]
    public Vector2 PreviousOffset { get; set; } = new(0, 0);

    [DataField] // Shitmed Change
    public EntityUid? SleepAction;
}