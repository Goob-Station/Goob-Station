using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Ranching.Food;

[RegisterComponent, NetworkedComponent]
public sealed partial class FoodSeedVisualsComponent : Component;

[Serializable, NetSerializable]
public enum SeedColor : byte
{
    Color,
}
