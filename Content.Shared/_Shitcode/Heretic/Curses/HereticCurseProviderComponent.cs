using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Shitcode.Heretic.Curses;

[RegisterComponent, NetworkedComponent]
public sealed partial class HereticCurseProviderComponent : Component
{
    [DataField]
    public float BloodTimeMultiplier = 4f;

    [DataField(required: true)]
    public Dictionary<EntProtoId, CurseProviderData> CursePrototypes;

    [DataField]
    public EntProtoId CursedStatusEffect = "StatusEffectCursed";

    [DataField]
    public TimeSpan CurseDelay = TimeSpan.FromMinutes(5);
}

[Serializable, NetSerializable, DataDefinition]
public sealed partial class CurseProviderData
{
    [DataField(required: true)]
    public TimeSpan Time = TimeSpan.Zero;

    [DataField]
    public bool Silent;
}
