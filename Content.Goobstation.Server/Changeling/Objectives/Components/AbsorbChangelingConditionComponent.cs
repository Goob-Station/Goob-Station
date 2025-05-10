using Content.Goobstation.Server.Changeling.Objectives.Systems;

namespace Content.Goobstation.Server.Changeling.Objectives.Components;

[RegisterComponent, Access(typeof(ChangelingObjectiveSystem), typeof(ChangelingSystem))]
public sealed partial class AbsorbChangelingConditionComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float LingAbsorbed = 0f;
}
