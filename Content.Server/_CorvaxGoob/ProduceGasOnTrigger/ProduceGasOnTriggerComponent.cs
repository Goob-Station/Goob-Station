using Content.Shared.Atmos;

namespace Content.Server._CorvaxGoob.ProduceGasOnTrigger;

[RegisterComponent]
public sealed partial class ProduceGasOnTriggerComponent : Component
{
    [DataField]
    public Dictionary<Gas, float>? Gases;

    [DataField("temp")]
    public float MixtureTemparature = 293.15f;
}
