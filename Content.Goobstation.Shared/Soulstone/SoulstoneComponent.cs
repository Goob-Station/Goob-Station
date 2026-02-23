using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Soulstone;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SoulstoneComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public EntityUid MindId;

    [DataField] public SoulstoneFamily Family;
}

// idk how to name it so let's stay at that.
public enum SoulstoneFamily
{
    Cult,
    Holy,
    Wizard
}
