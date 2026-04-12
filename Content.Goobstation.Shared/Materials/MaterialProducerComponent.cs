using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Content.Shared.Materials;

namespace Content.Goobstation.Shared.Materials;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause]
public sealed partial class MaterialProducerComponent : Component
{
    /// <summary>
    /// How much and what type of materials should be produced.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<MaterialPrototype>, int> MaterialsProduce = new();

    /// <summary>
    /// Should materials be produced when there is no electricity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsWorkingWithoutElectricity = false;

    /// <summary>
    /// What is the time interval between production.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan ProductionDelay = TimeSpan.FromSeconds(60);

    /// <summary>
    /// When should the next batch of materials be produced.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField, AutoNetworkedField]
    public TimeSpan NextProduceTime = TimeSpan.Zero;
}
