using Content.Server.NPC.HTN;

namespace Content.Trauma.Server.Ranching;

[RegisterComponent]
public sealed partial class HostileWhenUnhappyComponent : Component
{
    [DataField]
    public float HappinessRequired = -10f;

    [DataField]
    public HTNCompoundTask UnhappyTask;

    [DataField]
    public HTNCompoundTask HappyTask;
}
