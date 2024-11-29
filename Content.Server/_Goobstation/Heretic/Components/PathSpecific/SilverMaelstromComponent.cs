namespace Content.Server.Heretic.Components.PathSpecific;

[RegisterComponent]
public sealed partial class SilverMaelstromComponent : Component
{
    [DataField] public float RespawnCooldown = 10f;
    [ViewVariables(VVAccess.ReadWrite)] public float RespawnTimer = 0f;

    [ViewVariables(VVAccess.ReadOnly)] public int ActiveBlades = 0;
    [DataField] public int MaxBlades = 5;
}
