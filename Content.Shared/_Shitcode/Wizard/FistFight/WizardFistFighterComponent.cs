using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Wizard.FistFight;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class WizardFistFighterComponent : Component
{
    [DataField, AutoNetworkedField]
    public FistFightCorner Corner;
}

[Serializable, NetSerializable]
public enum FistFightCorner : byte
{
    Red,
    Blue,
}
