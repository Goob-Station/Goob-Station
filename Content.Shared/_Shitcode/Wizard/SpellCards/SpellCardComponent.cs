using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Wizard.SpellCards;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SpellCardComponent : Component
{
    [AutoNetworkedField]
    public EntityUid? Target;

    [AutoNetworkedField]
    public bool Targeted;

    [AutoNetworkedField]
    public bool Flipped;

    [DataField]
    public float TargetedSpeed = 20f;

    [DataField]
    public float FlipTime = 0.4f;

    [DataField]
    public float Tolerance = 0.1f;

    [DataField]
    public Color FlippedTrailColor = Color.White;

    [ViewVariables(VVAccess.ReadOnly)]
    public float FlipAccumulator;

    [DataField]
    public float RotateTime = 0.1f;

    [ViewVariables(VVAccess.ReadOnly)]
    public float RotateAccumulator;
}

[Serializable, NetSerializable]
public enum SpellCardVisuals : byte
{
    State // 0 - not flipped, 1 - flipping, 2 - flipped
}
