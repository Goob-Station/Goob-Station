using System.Numerics;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Server.SpecialPassives.SpontaneousCombustion;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class SpontaneousCombustionComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextCombustion;

    [DataField]
    public Vector2 CombustionRangeSeconds = new(300f, 1500f);

    [DataField]
    public Vector2 FireStackRange = new(2f, 8f);

    [DataField]
    public LocId MessageSelf = "spontaneous-combustion-component-message-self";

    [DataField]
    public LocId MessageOthers = "spontaneous-combustion-component-message-others";
}
