using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.AlmanacBlade;

[RegisterComponent]
public sealed partial class AlmanacBladeArtifactSpawnerComponent : Component
{
    [DataField]
    public EntProtoId Blade = "WeaponAlmanacBlade";

    [DataField]
    public float Chance = 0.01f;
}

[RegisterComponent]
public sealed partial class AlmanacBladeChestSpawnerComponent : Component
{
    [DataField]
    public EntProtoId Blade = "WeaponAlmanacBlade";

    [DataField]
    public float Chance = 0.01f;
}

[RegisterComponent]
public sealed partial class AlmanacOreCounterComponent : Component;
