using Content.Shared.Stunnable;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Server.Stunnable.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
[Access(typeof(SharedStunbatonSystem))]
public sealed partial class StunbatonComponent : Component
{
    [DataField("energyPerUse"), ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public float EnergyPerUse = 350;

    [DataField, AutoNetworkedField]
    public float LightAttackEnergyMultiplier = 1f;

    [DataField("sparksSound")]
    public SoundSpecifier SparksSound = new SoundCollectionSpecifier("sparks");
}
