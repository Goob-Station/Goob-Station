using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.MartialArts.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MeleeAttackRateMultiplierComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public List<MeleeAttackRateMultiplierData> Data = new();

    [DataField]
    public float MinMultiplier = 0.3f;

    [DataField]
    public float MaxMultiplier = 2f;
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class MeleeAttackRateMultiplierData
{
    public float Multiplier = 1f;

    public TimeSpan EndTime = TimeSpan.Zero;
}
