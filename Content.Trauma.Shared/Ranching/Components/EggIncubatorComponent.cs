// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Trauma.Shared.Ranching.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class EggIncubatorComponent : Component;

[Serializable, NetSerializable]
public enum EggIncubatorVisuals : byte
{
    Egg,
}
