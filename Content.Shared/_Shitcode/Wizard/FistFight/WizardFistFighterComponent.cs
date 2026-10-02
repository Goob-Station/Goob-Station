using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Wizard.FistFight;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class WizardFistFighterComponent : Component
{
    [DataField, AutoNetworkedField]
    public FistFightCorner Corner;

    [ViewVariables(VVAccess.ReadOnly)]
    public int Score;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? Gloves;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? OriginalGloves;

    [ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid> StoredItems = new();

    [ViewVariables(VVAccess.ReadOnly)]
    public bool WasPacified;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan? PacifiedUntil;
}

[Serializable, NetSerializable]
public enum FistFightCorner : byte
{
    Red,
    Blue,
}
