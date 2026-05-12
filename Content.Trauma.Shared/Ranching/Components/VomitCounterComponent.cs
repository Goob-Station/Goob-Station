// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Trauma.Shared.Ranching.Components;

/// <summary>
/// The question isn't what is this used for the question is will anyone else use this?
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class VomitCounterComponent : Component
{
    [DataField]
    public int TimesVomited;

    [DataField]
    public int NeededVomits;
}
