// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Goobstation.Common.DoAfter;

[RegisterComponent]
public sealed partial class DoAfterDelayMultiplierComponent : Component
{
    [DataField]
    public float Multiplier = 1f;
}