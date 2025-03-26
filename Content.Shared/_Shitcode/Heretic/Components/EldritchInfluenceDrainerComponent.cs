using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class EldritchInfluenceDrainerComponent : Component
{
    [DataField]
    public float Time = 8f;

    [DataField]
    public float KnowledgePerInfluence = 2f;

    [DataField]
    public bool Hidden;
}
