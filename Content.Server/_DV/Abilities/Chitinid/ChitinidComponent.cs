using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Abilities.Chitinid;

[RegisterComponent]
public sealed partial class ChitinidComponent : Component
{
    [DataField]
    public EntProtoId ChitzitePrototype = "Chitzite";

    [DataField]
    public EntProtoId ChitziteActionId = "ActionChitzite";

    [DataField]
    public EntityUid? ChitziteAction;

    [DataField]
    public FixedPoint2 AmountAbsorbed = 0f;

    [DataField]
    public DamageSpecifier Healing = new()
    {
        DamageDict = new()
        {
            { "Radiation", -0.5 },
        }
    };

    [DataField]
    public FixedPoint2 MaximumAbsorbed = 30f;

    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextUpdate;
}
