// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
namespace Content.Goobstation.Shared._Trauma.Ranching.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class EggIncubatorComponent : Component;

[Serializable, NetSerializable]
public enum EggIncubatorVisuals : byte
{
    Egg,
}
