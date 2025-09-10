using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Silicon.MalfAI.Components;

[RegisterComponent]
public sealed partial class RoboticFactoryComponent : Component
{
    [DataField]
    public bool Powered = false;

    [DataField]
    public string FixtureId = "area";

    [DataField]
    public EntProtoId BorgProtoId = "PlayerBorgBattery";

    [DataField]
    public EntityWhitelist Whitelist = new();

    [DataField]
    public EntityWhitelist Blacklist = new();

    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(3);

    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan? FinishTime;

    [ViewVariables(VVAccess.ReadWrite)]
    public Container ConversionContainer = default!;

    [ViewVariables(VVAccess.ReadOnly)]
    public string ContainerID = "robotic-factory-container";
}