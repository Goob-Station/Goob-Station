using Content.Goobstation.Shared.Terror.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Terror.Components;

/// <summary>
/// Growth code for the spooderling. Cause I think it's cooler for the ghostrole to be on the spiderlings.
/// </summary>
[RegisterComponent]
public sealed partial class TerrorSpiderlingComponent : Component
{
    [DataField(required: true)]
    public ProtoId<TerrorSpiderPrototype> GrowsInto;

    [DataField]
    public TimeSpan GrowDelay = TimeSpan.FromSeconds(45);

    [DataField]
    public TimeSpan GrowAt;
}
