using Content.Server.Heretic;
using Content.Server.Objectives.Systems;

namespace Content.Server.Objectives.Components;

[RegisterComponent, Access(typeof(HereticObjectiveSystem), typeof(HereticSystem))]
public sealed partial class HereticKnowledgeConditionComponent : Component
{
    [DataField] public float Researched = 0f;
}
