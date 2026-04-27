namespace Content.Trauma.Shared.StatusEffects;

[RegisterComponent]
public sealed partial class AddShaderStatusEffectComponent : Component
{
    [DataField(required: true)]
    public string Shader;
}
