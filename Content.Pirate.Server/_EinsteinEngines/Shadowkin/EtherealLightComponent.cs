namespace Content.Server.Shadowkin;

[RegisterComponent]
public sealed partial class EtherealLightComponent : Component
{
    public EntityUid AttachedEntity = EntityUid.Invalid;

    public float OldRadius = 0f;

    public bool OldRadiusEdited = false;

    public float OldEnergy = 0f;

    public bool OldEnergyEdited = false;
}