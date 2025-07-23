using Content.Server.Objectives.Systems;

namespace Content.Server.Objectives.Components;

// Automatically an assign objective as complete.

[RegisterComponent, Access(typeof(AutoCompleteConditionSystem))]
public sealed partial class AutoCompleteConditionComponent : Component
{
}
