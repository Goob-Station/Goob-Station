using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Components.Mobs;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class EatFilthComponent : Component
{
    /// <summary>
    /// Used to keep track of how much trash the rat has eaten.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int FilthConsumed;
}

[ByRefEvent]
public record struct AteFilthEvent(int CurrentFilthConsumed);
