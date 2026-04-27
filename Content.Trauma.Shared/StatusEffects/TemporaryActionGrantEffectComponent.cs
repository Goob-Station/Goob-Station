namespace Content.Trauma.Shared.StatusEffects;

[RegisterComponent, NetworkedComponent]
public sealed partial class TemporaryActionGrantEffectComponent : Component
{
    [DataField]
    public List<EntityUid> Actions = new ();

    [DataField(required: true)]
    public List<EntProtoId> ActionPrototypes;
}
