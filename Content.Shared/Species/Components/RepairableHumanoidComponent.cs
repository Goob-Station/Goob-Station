/// <summary>
/// this is used for a speicies that can be repaired with welders, like IPCs.
/// </summary>
/// all the actual component does is get detected by RepairableSystem.

namespace Content.Shared.Silicons.Components;

/// <summary>
/// This is used for synthetic species like IPCs that have humanoid bodies
/// rather than standard mechanical Borg chassis.
/// </summary>
[RegisterComponent]
public sealed partial class RepairableHumanoidComponent : Component
{
}
