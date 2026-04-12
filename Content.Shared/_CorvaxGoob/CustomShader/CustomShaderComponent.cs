using Robust.Shared.GameStates;

namespace Content.Shared._CorvaxGoob.CustomShader;

/// <summary>
/// Применяет указанный в нём шейдер на сущность.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class CustomShaderComponent : Component
{
    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public string? Shader { get; set; }
}
