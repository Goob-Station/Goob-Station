using Robust.Shared.GameStates;

namespace Content.Shared._CorvaxGoob.Skills;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SkillsComponent : Component
{
    [DataField, AutoNetworkedField]
    public HashSet<Skills> Skills = [];
}
