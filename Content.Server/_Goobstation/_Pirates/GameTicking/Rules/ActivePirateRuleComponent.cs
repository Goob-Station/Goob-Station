namespace Content.Server._Goobstation._Pirates.GameTicking.Rules;

[RegisterComponent]
public sealed partial class ActivePirateRuleComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)] public float Credits = 0f;
    [ViewVariables(VVAccess.ReadWrite)] public EntityUid? BoundSiphon;
}
