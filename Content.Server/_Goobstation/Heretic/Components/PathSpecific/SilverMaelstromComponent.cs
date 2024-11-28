namespace Content.Server.Heretic.Components.PathSpecific;

[RegisterComponent]
public sealed partial class SilverMaelstromComponent : Component
{
    [DataField] public float RespawnCooldown = 5f;
    [ViewVariables(VVAccess.ReadWrite)] public float RespawnTimer = 0f;

    [ViewVariables(VVAccess.ReadOnly)] public int ActiveBlades = 0;
    [DataField] public int MaxBlades = 10;
}
