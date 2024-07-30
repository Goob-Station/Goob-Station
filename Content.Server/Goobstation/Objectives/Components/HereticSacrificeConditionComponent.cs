using Content.Server.Objectives.Systems;

namespace Content.Server.Goobstation.Objectives.Components;

[RegisterComponent, Access(typeof(HereticObjectiveSystem))]
public sealed partial class HereticSacrificeConditionComponent : Component
{
    [DataField] public float Sacrificed = 0f;
    /// <summary>
    ///     Indicates that a victim should be a head role / command.
    /// </summary>
    [DataField] public bool IsCommand = false;
}
