// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Oskarrr.EngineerJockey;

/// <summary>
/// Ancient engineer console that can remotely initiate cryopod awakening.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class EngineerConsoleComponent : Component
{
    [DataField]
    public float Range = 25f;
}
