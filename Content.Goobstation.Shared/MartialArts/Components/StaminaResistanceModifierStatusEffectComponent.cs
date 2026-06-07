using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.MartialArts.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StaminaResistanceModifierStatusEffectComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Modifier = 1f;
}
