// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Medical.Surgery;

[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryTargetComponent : Component
{
    [DataField]
    public bool CanOperate = true;

    /// <summary>
    /// Should be self-explanatory. Is used to process logic of dealing poison damage to a skeleton.
    /// </summary>
    [DataField]
    public bool SepsisImmune;
}
