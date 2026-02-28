using Robust.Shared.GameStates;

namespace Content.Shared._White.Xenomorphs.FaceHugger;

/// <summary>
/// This is probably some sort of coding crime but this is literally only used to so facehuggers can see the larva infection icons.
/// Just giving them the xenomorph component breaks the entity and I don't wanna bugfix even moreee.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class LarvaInfestationVisionComponent : Component
{
}
